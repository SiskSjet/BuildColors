using System;
using System.Collections.Generic;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// The shares waiting for the player. Nothing here is stored until the player takes it, so the
    /// list on disk stays theirs alone.
    /// </summary>
    public class ShareInbox {
        /// <summary>
        /// Oldest offers are dropped past this, so a busy server cannot grow the list without end.
        /// </summary>
        public const int MAX_OFFERS = 20;

        private readonly List<ShareOffer> _offers = new List<ShareOffer>();

        /// <summary>
        /// Raised whenever an offer arrives or leaves.
        /// </summary>
        public event Action Changed;

        public int Count {
            get { return _offers.Count; }
        }

        /// <summary>
        /// Newest first.
        /// </summary>
        public IReadOnlyList<ShareOffer> Offers {
            get { return _offers; }
        }

        /// <summary>
        /// Takes a share in, replacing an earlier one of the same name from the same player.
        /// </summary>
        public void Add(ShareOffer offer) {
            if (offer == null || !offer.IsValid) {
                return;
            }

            _offers.RemoveAll(existing => existing.Kind == offer.Kind
                && StringComparer.InvariantCultureIgnoreCase.Equals(existing.Name, offer.Name)
                && StringComparer.InvariantCultureIgnoreCase.Equals(existing.SenderName, offer.SenderName));

            _offers.Insert(0, offer);

            while (_offers.Count > MAX_OFFERS) {
                _offers.RemoveAt(_offers.Count - 1);
            }

            Raise();
        }

        /// <summary>
        /// Saves the offer under a name that is free, and returns that name.
        /// </summary>
        public bool Accept(ShareOffer offer, out string name) {
            name = null;

            if (offer == null || !offer.IsValid || !_offers.Contains(offer)) {
                return false;
            }

            var mod = Mod.Static;
            if (mod == null) {
                return false;
            }

            if (offer.Kind == ShareKind.PaintJob) {
                var service = mod.PaintJobService;
                if (service == null) {
                    return false;
                }

                var job = offer.PaintJob.Clone();

                job.Id = Guid.NewGuid();
                job.Name = service.UniqueJobName(offer.PaintJob.Name);

                service.SaveJob(job);
                name = job.Name;
            } else {
                var set = offer.ColorSet.Upgraded().WithName(mod.UniqueColorSetName(offer.ColorSet.Name));
                set.CreatedTicks = 0L;

                mod.SaveColorSet(set);
                name = set.Name;
            }

            _offers.Remove(offer);
            Raise();

            return true;
        }

        public bool Decline(ShareOffer offer) {
            if (offer == null || !_offers.Remove(offer)) {
                return false;
            }

            Raise();

            return true;
        }

        public void Clear() {
            if (_offers.Count == 0) {
                return;
            }

            _offers.Clear();
            Raise();
        }

        /// <summary>
        /// The offer at a one based position, as the chat commands list them.
        /// </summary>
        public ShareOffer At(int position) {
            return position >= 1 && position <= _offers.Count ? _offers[position - 1] : null;
        }

        private void Raise() {
            var handler = Changed;

            if (handler != null) {
                handler();
            }
        }
    }
}
