using ProtoBuf;
using System.Xml.Serialization;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    /// <summary>
    /// One choice a patterned source can land on.
    /// </summary>
    [ProtoContract]
    public class PaintPaletteEntry {
        public PaintPaletteEntry() { }

        public PaintPaletteEntry(ColorModel color) {
            Color = color;
        }

        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public ColorModel Color { get; set; }

        [ProtoMember(2)]
        [XmlAttribute("skin")]
        public string SkinId { get; set; }

        /// <summary>
        /// Relative share of the blocks this entry takes.
        /// </summary>
        [ProtoMember(3)]
        [XmlAttribute("weight")]
        public float Weight { get; set; } = 1f;

        public PaintPaletteEntry Clone() {
            return new PaintPaletteEntry {
                Color = Color,
                SkinId = SkinId,
                Weight = Weight
            };
        }
    }
}
