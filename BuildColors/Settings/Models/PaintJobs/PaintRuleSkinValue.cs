using ProtoBuf;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public struct PaintRuleSkinValue {
        [ProtoMember(1)]
        [XmlAttribute("id")]
        public string SkinId { get; set; }

        [ProtoMember(2)]
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }
    }
}
