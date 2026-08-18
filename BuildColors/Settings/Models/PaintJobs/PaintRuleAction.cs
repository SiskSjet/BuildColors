using ProtoBuf;
using System.Xml.Serialization;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public class PaintRuleAction {
        private bool _hasHsv;
        private SeHsv _hsv;

        [ProtoMember(6)]
        [XmlElement]
        public SeHsv TargetHsv {
            get { return _hsv; }
            set {
                _hsv = value;
                _hasHsv = true;
            }
        }

        /// <summary>
        /// RGB as written before SE HSV. Read so old files still paint the same, never written again.
        /// </summary>
        [ProtoMember(1)]
        [XmlElement("TargetColor")]
        public ColorModel LegacyTargetColor {
            get { return default(ColorModel); }
            set {
                if (!_hasHsv) {
                    TargetHsv = (SeHsv)ColorMask.FromColor(value);
                }
            }
        }

        [XmlIgnore]
        public ColorMask TargetColor {
            get { return TargetHsv; }
            set { TargetHsv = value; }
        }

        [ProtoMember(2)]
        [XmlAttribute("applyColor")]
        public bool ApplyColor { get; set; } = true;

        [ProtoMember(3)]
        [XmlAttribute("skin")]
        public string TargetSkinId { get; set; }

        [ProtoMember(4)]
        [XmlAttribute("applySkin")]
        public bool ApplySkin { get; set; }

        /// <summary>
        /// How the color and skin are derived per block.
        /// </summary>
        [ProtoMember(5)]
        [XmlElement]
        public PaintColorSource Source { get; set; }

        /// <summary>
        /// Type of the source, treating a missing one as solid.
        /// </summary>
        public PaintSourceType SourceType {
            get { return Source != null ? Source.Type : PaintSourceType.Solid; }
        }

        public bool ShouldSerializeLegacyTargetColor() {
            return false;
        }

        /// <summary>
        /// Creates an independent copy.
        /// </summary>
        public PaintRuleAction Clone() {
            return new PaintRuleAction {
                TargetHsv = TargetHsv,
                ApplyColor = ApplyColor,
                TargetSkinId = TargetSkinId,
                ApplySkin = ApplySkin,
                Source = Source != null ? Source.Clone() : null
            };
        }
    }
}
