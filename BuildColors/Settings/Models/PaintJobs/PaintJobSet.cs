using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    [XmlRoot(nameof(PaintJobSet))]
    public class PaintJobSet : HashSet<PaintJob> {
        public const int VERSION = 1;

        public PaintJobSet() : base(new PaintJobComparer()) { }

        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public int Version { get; set; } = VERSION;

        /// <summary>
        ///     The job the hotkeys act on. Kept with the jobs rather than in a separate setting so that it
        ///     survives a reload alongside them, and so a job that has been deleted simply stops resolving.
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute("active")]
        public Guid ActiveJobId { get; set; }
    }
}
