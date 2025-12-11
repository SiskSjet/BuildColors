using ProtoBuf;
using Sisk.BuildColors.Settings.Models.ColorSpace;
using System.Xml.Serialization;
using VRageMath;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings.Models {

    [ProtoContract]
    public struct Color {

        public Color(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
        }

        [ProtoMember(3)]
        [XmlAttribute("b")]
        public byte B { get; set; }

        [ProtoMember(2)]
        [XmlAttribute("g")]
        public byte G { get; set; }

        [ProtoMember(1)]
        [XmlAttribute("r")]
        public byte R { get; set; }

        public static implicit operator VRageMath.Color(Color color) {
            return new VRageMath.Color(color.R, color.G, color.B);
        }

        public static implicit operator Color(VRageMath.Color color) {
            return new Color(color.R, color.G, color.B);
        }

        public static implicit operator Color(Vector3 vector) {
            return vector.ColorMaskToHSV().HSVtoColor();
        }

        public static implicit operator Color(HSL color) {
            var rgb = color.ToRGB();
            return new Color(rgb.R, rgb.G, rgb.B);
        }

        public static implicit operator Color(RGB color) {
            return new Color(color.R, color.G, color.B);
        }

        public static implicit operator HSL(Color color) {
            return new RGB(color.R, color.G, color.B).ToHSL();
        }

        public static implicit operator RGB(Color color) {
            return new RGB(color.R, color.G, color.B);
        }

        public static implicit operator Vector3(Color color) {
            return VRageMath.ColorExtensions.ColorToHSV(color).HSVToColorMask();
        }
    }
}