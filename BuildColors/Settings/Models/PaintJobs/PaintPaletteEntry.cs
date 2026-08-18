using ProtoBuf;
using System.Xml.Serialization;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    /// <summary>
    /// One choice a patterned source can land on.
    /// </summary>
    [ProtoContract]
    public class PaintPaletteEntry {
        private bool _hasHsv;
        private SeHsv _hsv;

        public PaintPaletteEntry() { }

        public PaintPaletteEntry(ColorMask color) {
            Color = color;
        }

        [ProtoMember(4)]
        [XmlElement]
        public SeHsv Hsv {
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

        [ProtoMember(2)]
        [XmlAttribute("skin")]
        public string SkinId { get; set; }

        /// <summary>
        /// Relative share of the blocks this entry takes.
        /// </summary>
        [ProtoMember(3)]
        [XmlAttribute("weight")]
        public float Weight { get; set; } = 1f;

        public bool ShouldSerializeLegacyColor() {
            return false;
        }

        public PaintPaletteEntry Clone() {
            return new PaintPaletteEntry {
                Hsv = Hsv,
                SkinId = SkinId,
                Weight = Weight
            };
        }
    }
}
