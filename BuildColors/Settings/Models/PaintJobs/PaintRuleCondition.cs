using ProtoBuf;
using System.Xml.Serialization;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    /// <summary>
    ///     A single test a block is put through. <see cref="Type" /> selects which of the value members is
    ///     used; the others are ignored, so a condition is always fully described by its type.
    /// </summary>
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
        public ColorModel Color { get; set; }

        [ProtoMember(4)]
        [XmlElement(Order = 4)]
        public PaintRuleDefinitionValue Definition { get; set; }

        [ProtoMember(5)]
        [XmlAttribute("skin")]
        public string SkinId { get; set; }

        [ProtoMember(6)]
        [XmlAttribute("category")]
        public PaintRuleBlockCategory Category { get; set; } = PaintRuleBlockCategory.Armor;

        [ProtoMember(7)]
        [XmlAttribute("gridSize")]
        public PaintRuleGridSize GridSize { get; set; } = PaintRuleGridSize.Large;

        [ProtoMember(8)]
        [XmlAttribute("integrity")]
        public PaintRuleIntegrityState Integrity { get; set; } = PaintRuleIntegrityState.Damaged;

        /// <summary>
        ///     Percentage the block integrity is held against, used by
        ///     <see cref="PaintRuleIntegrityState.BelowThreshold" /> and ignored by the other states.
        /// </summary>
        [ProtoMember(9)]
        [XmlAttribute("integrityThreshold")]
        public float IntegrityThreshold { get; set; } = 50f;

        /// <summary>
        ///     Creates an independent copy. All value holders are structs, so a member wise copy is enough.
        /// </summary>
        public PaintRuleCondition Clone() {
            return new PaintRuleCondition {
                Type = Type,
                Comparison = Comparison,
                Color = Color,
                Definition = Definition,
                SkinId = SkinId,
                Category = Category,
                GridSize = GridSize,
                Integrity = Integrity,
                IntegrityThreshold = IntegrityThreshold
            };
        }
    }
}
