using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace Sisk.BuildColors {

    public static class ColorExtensions {

        public static float GetBrightness(this Color color) {
            float hue, saturation, brightness;
            RGBtoHSB(color.R, color.G, color.B, out hue, out saturation, out brightness);
            return brightness;
        }

        public static float GetHue(this Color color) {
            float hue, saturation, brightness;
            RGBtoHSB(color.R, color.G, color.B, out hue, out saturation, out brightness);
            return hue;
        }

        public static float GetSaturation(this Color color) {
            float hue, saturation, brightness;
            RGBtoHSB(color.R, color.G, color.B, out hue, out saturation, out brightness);
            return saturation;
        }

        public static void RGBtoHSB(int r, int g, int b, out float hue, out float saturation, out float brightness) {
            float h, s, v;

            h = 0;
            if (r == g && g == b) {
                h = 0;
            } else if (r >= g && g >= b) {
                h = 60 * (g - b) / (float)(r - b);
            } else if (g > r && r >= b) {
                h = 60 * (2 - (r - b) / (float)(g - b));
            } else if (g >= b && b > r) {
                h = 60 * (2 + (b - r) / (float)(g - r));
            } else if (b > g && g > r) {
                h = 60 * (4 - (g - r) / (float)(b - r));
            } else if (b > r && r >= g) {
                h = 60 * (4 + (r - g) / (float)(b - g));
            }

            if (h < 0)
                h += 360;

            v = Math.Max(r, Math.Max(g, b));
            s = (v == 0) ? 0 : (1 - ((float)Math.Min(r, Math.Min(g, b))) / v);

            hue = h;
            saturation = s;
            brightness = v / 255f;
        }
    }
}