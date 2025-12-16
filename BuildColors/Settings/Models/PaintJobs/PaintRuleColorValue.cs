using ProtoBuf;
using Sisk.BuildColors.Settings.Models;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public struct PaintRuleColorValue {
        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public Color Value { get; set; }

        [ProtoMember(2)]
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }
    }
}
