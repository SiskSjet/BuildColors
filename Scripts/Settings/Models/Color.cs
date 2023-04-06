using ProtoBuf;
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

        [ProtoMember(1)]
        [XmlAttribute("r")]
        public byte R { get; set; }

        [ProtoMember(2)]
        [XmlAttribute("g")]
        public byte G { get; set; }

        [ProtoMember(3)]
        [XmlAttribute("b")]
        public byte B { get; set; }

        public static implicit operator VRageMath.Color(Color color) {
            return new VRageMath.Color(color.R, color.G, color.B);
        }

        public static implicit operator Color(VRageMath.Color color) {
            return new Color(color.R, color.G, color.B);
        }

        public static implicit operator Vector3(Color color) {
            return VRageMath.ColorExtensions.ColorToHSV(color).HSVToColorMask();
        }

        public static implicit operator Color(Vector3 vector) {
            return vector.ColorMaskToHSV().HSVtoColor();
        }
    }
}
