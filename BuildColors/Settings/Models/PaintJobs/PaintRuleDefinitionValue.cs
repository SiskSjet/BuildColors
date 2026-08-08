using ProtoBuf;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    /// <summary>
    ///     Block definition to match. Both fields accept the wildcards <c>*</c> (any number of characters)
    ///     and <c>?</c> (exactly one), so a single condition can cover a whole family of blocks. An empty
    ///     field matches any value.
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
