using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.ColorSpace;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using VRageMath;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Mixes two paint colors.
    /// </summary>
    internal static class PaintColorBlend {
        private const float ACHROMATIC_SATURATION = .01f;

        /// <summary>
        /// Mixes two slots, handing back the ends untouched rather than round tripping them.
        /// </summary>
        public static ColorMask Lerp(ColorMask from, ColorMask to, float amount, PaintBlendSpace space) {
            var t = MathHelper.Clamp(amount, 0f, 1f);

            if (t <= 0f) {
                return from;
            }

            if (t >= 1f) {
                return to;
            }

            ColorModel start = from.ToDisplayColor();
            ColorModel end = to.ToDisplayColor();

            switch (space) {
                case PaintBlendSpace.Hsv:
                    return ColorMask.FromColor(LerpHsv(start, end, t));

                case PaintBlendSpace.Lab:
                    return ColorMask.FromColor(LerpLab(start, end, t));

                default:
                    return ColorMask.FromColor(LerpRgb(start, end, t));
            }
        }

        private static ColorModel LerpHsv(ColorModel from, ColorModel to, float t) {
            var start = new RGB(from.R, from.G, from.B).ToHSV();
            var end = new RGB(to.R, to.G, to.B).ToHSV();

            var startHue = start.H;
            var endHue = end.H;

            if (start.S < ACHROMATIC_SATURATION) {
                startHue = endHue;
            } else if (end.S < ACHROMATIC_SATURATION) {
                endHue = startHue;
            }

            var hue = startHue + ShortestHueDelta(startHue, endHue) * t;
            if (hue < 0f) {
                hue += 360f;
            } else if (hue >= 360f) {
                hue -= 360f;
            }

            var blended = new HSV(hue, MathHelper.Lerp(start.S, end.S, t), MathHelper.Lerp(start.V, end.V, t)).ToRGB();

            return new ColorModel(blended.R, blended.G, blended.B);
        }

        private static ColorModel LerpRgb(ColorModel from, ColorModel to, float t) {
            return new ColorModel(
                LerpByte(from.R, to.R, t),
                LerpByte(from.G, to.G, t),
                LerpByte(from.B, to.B, t));
        }

        private static ColorModel LerpLab(ColorModel from, ColorModel to, float t) {
            var start = new RGB(from.R, from.G, from.B).ToLab();
            var end = new RGB(to.R, to.G, to.B).ToLab();

            var blended = new Lab(
                MathHelper.Lerp(start.L, end.L, t),
                MathHelper.Lerp(start.A, end.A, t),
                MathHelper.Lerp(start.B, end.B, t)).ToRGB();

            return new ColorModel(blended.R, blended.G, blended.B);
        }

        /// <summary>
        /// Signed distance from one hue to another the short way round.
        /// </summary>
        private static float ShortestHueDelta(float from, float to) {
            var delta = to - from;

            while (delta > 180f) {
                delta -= 360f;
            }

            while (delta < -180f) {
                delta += 360f;
            }

            return delta;
        }

        private static byte LerpByte(byte from, byte to, float t) {
            return (byte)MathHelper.Clamp(from + (to - from) * t + .5f, 0f, 255f);
        }
    }
}
