using ProtoBuf;
using System.Xml.Serialization;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    /// <summary>
    /// One end of a gradient leg.
    /// </summary>
    [ProtoContract]
    public class PaintColorStop {
        private bool _hasHsv;
        private SeHsv _hsv;

        public PaintColorStop() { }

        public PaintColorStop(ColorMask color, float position) {
            Color = color;
            Position = position;
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
        [XmlAttribute("position")]
        public float Position { get; set; }

        [ProtoMember(3)]
        [XmlAttribute("skin")]
        public string SkinId { get; set; }

        public bool ShouldSerializeLegacyColor() {
            return false;
        }

        public PaintColorStop Clone() {
            return new PaintColorStop {
                Hsv = Hsv,
                Position = Position,
                SkinId = SkinId
            };
        }
    }
}
