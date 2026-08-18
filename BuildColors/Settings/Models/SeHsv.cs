using ProtoBuf;
using System;
using System.Xml.Serialization;
using VRageMath;

namespace Sisk.BuildColors.Settings.Models {

    /// <summary>
    /// A build color slot as Space Engineers' own color picker shows it, and the one shape every saved
    /// color is kept in. <see cref="ColorMask"/> is what the game works in, RGB <see cref="Color"/> only
    /// command input, display and older files.
    /// </summary>
    [ProtoContract]
    public struct SeHsv {
        public SeHsv(float hue, float saturation, float value) {
            H = hue;
            S = saturation;
            V = value;
        }

        [ProtoMember(1)]
        [XmlAttribute("h")]
        public float H { get; set; }

        [ProtoMember(2)]
        [XmlAttribute("s")]
        public float S { get; set; }

        [ProtoMember(3)]
        [XmlAttribute("v")]
        public float V { get; set; }

        public static implicit operator ColorMask(SeHsv hsv) {
            var normalized = new Vector3(hsv.H / 360f, hsv.S / 100f, hsv.V / 100f);

            return normalized.HSVToColorMask();
        }

        public static implicit operator SeHsv(ColorMask mask) {
            var hsv = ((Vector3)mask).ColorMaskToHSV();

            return new SeHsv(
                (float)Math.Round(hsv.X * 360f, 2),
                (float)Math.Round(hsv.Y * 100f, 2),
                (float)Math.Round(hsv.Z * 100f, 2));
        }
    }
}
