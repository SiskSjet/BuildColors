using ProtoBuf;
using Sisk.BuildColors.Settings.Models;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public class PaintRuleAction {
        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public Color TargetColor { get; set; }

        [ProtoMember(2)]
        [XmlAttribute("applyColor")]
        public bool ApplyColor { get; set; } = true;

        [ProtoMember(3)]
        [XmlElement(Order = 3)]
        public PaintRuleSkinValue TargetSkin { get; set; }

        [ProtoMember(4)]
        [XmlAttribute("applySkin")]
        public bool ApplySkin { get; set; }

        /// <summary>
        ///     Creates an independent copy.
        /// </summary>
        public PaintRuleAction Clone() {
            return new PaintRuleAction {
                TargetColor = TargetColor,
                ApplyColor = ApplyColor,
                TargetSkin = TargetSkin,
                ApplySkin = ApplySkin
            };
        }
    }
}
