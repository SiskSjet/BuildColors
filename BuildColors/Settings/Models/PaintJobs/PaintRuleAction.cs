using ProtoBuf;
using System.Xml.Serialization;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public class PaintRuleAction {
        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public ColorModel TargetColor { get; set; }

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
        [XmlElement(Order = 5)]
        public PaintColorSource Source { get; set; }

        /// <summary>
        /// Type of the source, treating a missing one as solid.
        /// </summary>
        public PaintSourceType SourceType {
            get { return Source != null ? Source.Type : PaintSourceType.Solid; }
        }

        /// <summary>
        /// Creates an independent copy.
        /// </summary>
        public PaintRuleAction Clone() {
            return new PaintRuleAction {
                TargetColor = TargetColor,
                ApplyColor = ApplyColor,
                TargetSkinId = TargetSkinId,
                ApplySkin = ApplySkin,
                Source = Source != null ? Source.Clone() : null
            };
        }
    }
}
