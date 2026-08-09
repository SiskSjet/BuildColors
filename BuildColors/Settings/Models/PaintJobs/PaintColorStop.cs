using ProtoBuf;
using System.Xml.Serialization;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    /// <summary>
    /// One end of a gradient leg.
    /// </summary>
    [ProtoContract]
    public class PaintColorStop {
        public PaintColorStop() { }

        public PaintColorStop(ColorModel color, float position) {
            Color = color;
            Position = position;
        }

        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public ColorModel Color { get; set; }

        [ProtoMember(2)]
        [XmlAttribute("position")]
        public float Position { get; set; }

        [ProtoMember(3)]
        [XmlAttribute("skin")]
        public string SkinId { get; set; }

        public PaintColorStop Clone() {
            return new PaintColorStop {
                Color = Color,
                Position = Position,
                SkinId = SkinId
            };
        }
    }
}
