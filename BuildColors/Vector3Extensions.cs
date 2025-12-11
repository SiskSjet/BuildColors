using VRageMath;

namespace Sisk.BuildColors {

    public static class Vector3Extensions {

        public static Vector3 WithBrightness(this Vector3 hsvColor, double brightness) {
            return new Vector3(hsvColor.X, hsvColor.Y, brightness);
        }

        public static Vector3 WithSaturation(this Vector3 hsvColor, double saturation) {
            return new Vector3(hsvColor.X, saturation, hsvColor.Z);
        }

        public static Vector3 WithValue(this Vector3 hsvColor, double value) {
            return new Vector3(hsvColor.X, hsvColor.Y, value);
        }
    }
}