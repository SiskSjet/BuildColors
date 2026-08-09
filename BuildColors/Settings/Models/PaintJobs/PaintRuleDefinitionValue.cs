using ProtoBuf;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    /// <summary>
    /// Block definition to match.
    /// </summary>
    [ProtoContract]
    public struct PaintRuleDefinitionValue {
        [ProtoMember(1)]
        [XmlAttribute("type")]
        public string TypeId { get; set; }

        [ProtoMember(2)]
        [XmlAttribute("subtype")]
        public string SubtypeId { get; set; }

        public override string ToString() {
            if (string.IsNullOrWhiteSpace(TypeId) && string.IsNullOrWhiteSpace(SubtypeId)) {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(SubtypeId)
                ? TypeId
                : $"{TypeId}/{SubtypeId}";
        }
    }
}
