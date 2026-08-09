using ProtoBuf;
using System.Xml.Serialization;
using VRageMath;

namespace Sisk.BuildColors.Settings.Models {

    /// <summary>
    /// A build color slot exactly as the game holds it: hue, and saturation and value as offsets.
    /// </summary>
    [ProtoContract]
    public struct ColorMask {
        public ColorMask(float hue, float saturation, float value) {
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

        public static implicit operator Vector3(ColorMask mask) {
            return new Vector3(mask.H, mask.S, mask.V);
        }

        public static implicit operator ColorMask(Vector3 vector) {
            return new ColorMask(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// The mask of a legacy stored color, for reading sets written before masks were kept.
        /// </summary>
        public static ColorMask FromColor(Color color) {
            return (Vector3)color;
        }

        /// <summary>
        /// The color this slot paints with, for display and for anything that works in RGB.
        /// </summary>
        public VRageMath.Color ToDisplayColor() {
            Color color = (Vector3)this;

            return color;
        }
    }
}
