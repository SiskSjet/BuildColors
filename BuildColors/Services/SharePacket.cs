using ProtoBuf;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.PaintJobs;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// What a share carries.
    /// </summary>
    public enum ShareKind {
        ColorSet = 0,
        PaintJob = 1,
    }

    /// <summary>
    /// One share on the wire, from the sender to the server and on to the recipient.
    /// </summary>
    [ProtoContract]
    public class SharePacket {
        [ProtoMember(1)]
        public ShareKind Kind { get; set; }

        /// <summary>
        /// Who it is meant for, or zero for everyone online.
        /// </summary>
        [ProtoMember(2)]
        public ulong Recipient { get; set; }

        /// <summary>
        /// Filled in by the server from the connection it arrived on, so it cannot be claimed.
        /// </summary>
        [ProtoMember(3)]
        public ulong SenderId { get; set; }

        [ProtoMember(4)]
        public string SenderName { get; set; }

        [ProtoMember(5)]
        public ColorSet ColorSet { get; set; }

        [ProtoMember(6)]
        public PaintJob PaintJob { get; set; }
    }
}
