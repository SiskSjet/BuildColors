using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// A share waiting for the player to take it or turn it down.
    /// </summary>
    public class ShareOffer {
        public ShareOffer(SharePacket packet) {
            Kind = packet.Kind;
            SenderName = string.IsNullOrEmpty(packet.SenderName) ? ModText.BC_Share_UnknownSender.GetString() : packet.SenderName;
            ColorSet = packet.ColorSet;
            PaintJob = packet.PaintJob;
            ReceivedAt = DateTime.Now;
        }

        public ShareKind Kind { get; private set; }

        public string SenderName { get; private set; }

        public ColorSet ColorSet { get; private set; }

        public PaintJob PaintJob { get; private set; }

        public DateTime ReceivedAt { get; private set; }

        /// <summary>
        /// The name the shared thing was saved under by whoever sent it.
        /// </summary>
        public string Name {
            get {
                if (Kind == ShareKind.PaintJob) {
                    return PaintJob != null ? PaintJob.Name : string.Empty;
                }

                return ColorSet.Name ?? string.Empty;
            }
        }

        /// <summary>
        /// True when the payload survived the trip and can be saved.
        /// </summary>
        public bool IsValid {
            get {
                if (Kind == ShareKind.PaintJob) {
                    return PaintJob != null && PaintJob.Rules != null && PaintJob.Rules.Count > 0;
                }

                return ColorSet.Masks != null && ColorSet.Masks.Length > 0;
            }
        }

        /// <summary>
        /// One line for the inbox list.
        /// </summary>
        public string Describe() {
            return Kind == ShareKind.PaintJob
                ? ModText.BC_Share_OfferPaintJob.GetString(Name, SenderName)
                : ModText.BC_Share_OfferColorSet.GetString(Name, SenderName);
        }

        /// <summary>
        /// The line under the preview, saying what taking it would add.
        /// </summary>
        public string DescribeDetail() {
            if (Kind == ShareKind.PaintJob) {
                var rules = PaintJob != null && PaintJob.Rules != null ? PaintJob.Rules.Count : 0;

                return ModText.BC_Share_DetailPaintJob.GetString(SenderName, rules);
            }

            return ModText.BC_Share_DetailColorSet.GetString(SenderName);
        }
    }
}
