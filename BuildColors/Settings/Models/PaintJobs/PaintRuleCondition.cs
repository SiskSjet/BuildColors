using ProtoBuf;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public class PaintRuleCondition {
        [ProtoMember(1)]
        [XmlAttribute("type")]
        public PaintRuleConditionType Type { get; set; } = PaintRuleConditionType.BlockColor;

        [ProtoMember(2)]
        [XmlAttribute("comparison")]
        public PaintRuleComparison Comparison { get; set; } = PaintRuleComparison.Equals;

        [ProtoMember(3)]
        [XmlElement(Order = 3)]
        public PaintRuleColorValue Color { get; set; }

        [ProtoMember(4)]
        [XmlElement(Order = 4)]
        public PaintRuleDefinitionValue Definition { get; set; }

        [ProtoMember(5)]
        [XmlElement(Order = 5)]
        public PaintRuleSkinValue Skin { get; set; }

        /// <summary>
        ///     Creates an independent copy. All value holders are structs, so a member wise copy is enough.
        /// </summary>
        public PaintRuleCondition Clone() {
            return new PaintRuleCondition {
                Type = Type,
                Comparison = Comparison,
                Color = Color,
                Definition = Definition,
                Skin = Skin
            };
        }
    }
}
