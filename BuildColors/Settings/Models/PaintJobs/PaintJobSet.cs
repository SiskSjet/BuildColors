using ProtoBuf;
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
    }
}
