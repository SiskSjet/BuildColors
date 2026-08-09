using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;
using System;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Ties the transport to the inbox: sends what the player picked, and announces what arrives.
    /// </summary>
    public static class ShareService {
        /// <summary>
        /// How long the arrival notice stays on screen.
        /// </summary>
        private const int NOTIFICATION_MS = 5000;

        /// <summary>
        /// Sends a share to one player, or to everyone online when no name is given.
        /// </summary>
        public static void Share(SharePacket packet, string playerName) {
            if (packet == null || !Ensure()) {
                return;
            }

            if (string.IsNullOrEmpty(playerName)) {
                packet.Recipient = ShareNetwork.EVERYONE;
                Send(packet, ModText.BC_Share_Everyone.GetString());

                return;
            }

            foreach (var recipient in ShareNetwork.GetRecipients()) {
                if (StringComparer.InvariantCultureIgnoreCase.Equals(recipient.Name, playerName)) {
                    Share(packet, recipient);
                    return;
                }
            }

            Show(ModText.BC_Share_NoSuchPlayer.GetString(playerName));
        }

        /// <summary>
        /// Sends a share to a player picked from the list.
        /// </summary>
        public static void Share(SharePacket packet, SharePlayer recipient) {
            if (packet == null || !Ensure()) {
                return;
            }

            packet.Recipient = recipient.Id;
            Send(packet, recipient.Id == ShareNetwork.EVERYONE ? ModText.BC_Share_Everyone.GetString() : recipient.Name);
        }

        /// <summary>
        /// Puts an arriving share in the inbox and says so, without touching anything saved.
        /// </summary>
        public static void Receive(SharePacket packet) {
            var inbox = Mod.Static?.Inbox;

            if (packet == null || inbox == null) {
                return;
            }

            var offer = new ShareOffer(packet);

            if (!offer.IsValid) {
                return;
            }

            inbox.Add(offer);

            var message = offer.Kind == ShareKind.PaintJob
                ? ModText.BC_Share_ReceivedPaintJob.GetString(offer.SenderName, offer.Name)
                : ModText.BC_Share_ReceivedColorSet.GetString(offer.SenderName, offer.Name);

            Show(message);
            MyAPIGateway.Utilities.ShowNotification(ModText.BC_Share_ReceivedNotification.GetString(message, Mod.Acronym), NOTIFICATION_MS);

            Mod.Static.RefreshShares();
        }

        /// <summary>
        /// Writes the waiting shares to chat, numbered the way accept and decline take them.
        /// </summary>
        public static void ListInbox() {
            var inbox = Mod.Static?.Inbox;

            if (inbox == null || inbox.Count == 0) {
                Show(ModText.BC_Share_InboxEmpty.GetString());
                return;
            }

            for (var i = 0; i < inbox.Offers.Count; i++) {
                Show(ModText.BC_Share_InboxEntry.GetString(i + 1, inbox.Offers[i].Describe()));
            }
        }

        public static void AcceptByReference(string reference) {
            var inbox = Mod.Static?.Inbox;
            var offer = Resolve(reference);

            if (inbox == null || offer == null) {
                return;
            }

            string name;
            if (inbox.Accept(offer, out name)) {
                Show(ModText.BC_Share_Accepted.GetString(name));
                Mod.Static.RefreshShares();
            }
        }

        public static void DeclineByReference(string reference) {
            var inbox = Mod.Static?.Inbox;
            var offer = Resolve(reference);

            if (inbox == null || offer == null) {
                return;
            }

            if (inbox.Decline(offer)) {
                Show(ModText.BC_Share_Declined.GetString(offer.Name));
                Mod.Static.RefreshShares();
            }
        }

        /// <summary>
        /// Reads a #2 style position or a shared name.
        /// </summary>
        private static ShareOffer Resolve(string reference) {
            var inbox = Mod.Static?.Inbox;

            if (inbox == null || inbox.Count == 0) {
                Show(ModText.BC_Share_InboxEmpty.GetString());
                return null;
            }

            var trimmed = (reference ?? string.Empty).Trim().Trim('"');

            if (string.IsNullOrEmpty(trimmed)) {
                Show(ModText.BC_Cmd_Usage_Accept.GetString(Mod.Acronym));
                return null;
            }

            int position;
            if (CommandArguments.TryParseSelector(trimmed, out position)) {
                var selected = inbox.At(position);

                if (selected == null) {
                    Show(ModText.BC_Share_NoSuchOffer.GetString(trimmed));
                }

                return selected;
            }

            foreach (var offer in inbox.Offers) {
                if (StringComparer.InvariantCultureIgnoreCase.Equals(offer.Name, trimmed)) {
                    return offer;
                }
            }

            Show(ModText.BC_Share_NoSuchOffer.GetString(trimmed));

            return null;
        }

        private static void Send(SharePacket packet, string recipientName) {
            var name = packet.Kind == ShareKind.PaintJob && packet.PaintJob != null
                ? packet.PaintJob.Name
                : packet.ColorSet.Name;

            Show(ShareNetwork.Send(packet)
                ? ModText.BC_Share_Sent.GetString(name, recipientName)
                : ModText.BC_Share_Failed.GetString(name));
        }

        /// <summary>
        /// True when there is a session to share in, with a word to chat when there is not.
        /// </summary>
        private static bool Ensure() {
            if (ShareNetwork.IsAvailable) {
                return true;
            }

            Show(ModText.BC_Share_NeedsMultiplayer.GetString());

            return false;
        }

        private static void Show(string message) {
            MyAPIGateway.Utilities.ShowMessage(Mod.NAME, message);
        }
    }
}
