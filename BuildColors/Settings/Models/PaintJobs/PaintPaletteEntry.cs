using ProtoBuf;
using System.Xml.Serialization;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    /// <summary>
    ///     One choice a patterned source can land on. Color and skin travel together so a single entry can
    ///     describe a whole look - rust patches want the rusty skin as much as the brown - and either half
    ///     is ignored when the action does not apply that channel.
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
        ///     Relative share of the blocks this entry takes. Only <see cref="PaintSourceType.Scatter" /> reads
        ///     it; the other sources pick by position, where a weight has no meaning.
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
