using ProtoBuf;
using System.Xml.Serialization;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    /// <summary>
    /// A single test a block is put through.
    /// </summary>
    [ProtoContract]
    public class PaintRuleCondition {
        private bool _hasHsv;
        private SeHsv _hsv;

        [ProtoMember(1)]
        [XmlAttribute("type")]
        public PaintRuleConditionType Type { get; set; } = PaintRuleConditionType.BlockColor;

        [ProtoMember(2)]
        [XmlAttribute("comparison")]
        public PaintRuleComparison Comparison { get; set; } = PaintRuleComparison.Equals;

        [ProtoMember(10)]
        [XmlElement]
        public SeHsv Hsv {
            get { return _hsv; }
            set {
                _hsv = value;
                _hasHsv = true;
            }
        }

        /// <summary>
        /// RGB as written before SE HSV. Read so old files still match the same blocks, never written again.
        /// </summary>
        [ProtoMember(3)]
        [XmlElement("Color")]
        public ColorModel LegacyColor {
            get { return default(ColorModel); }
            set {
                if (!_hasHsv) {
                    Hsv = (SeHsv)ColorMask.FromColor(value);
                }
            }
        }

        [XmlIgnore]
        public ColorMask Color {
            get { return Hsv; }
            set { Hsv = value; }
        }

        [ProtoMember(4)]
        [XmlElement]
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
        /// Percentage the block integrity is held against, used by BelowThreshold and ignored by the other states.
        /// </summary>
        [ProtoMember(9)]
        [XmlAttribute("integrityThreshold")]
        public float IntegrityThreshold { get; set; } = 50f;

        public bool ShouldSerializeLegacyColor() {
            return false;
        }

        /// <summary>
        /// Creates an independent copy.
        /// </summary>
        public PaintRuleCondition Clone() {
            return new PaintRuleCondition {
                Type = Type,
                Comparison = Comparison,
                Hsv = Hsv,
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
