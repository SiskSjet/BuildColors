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
        private const float ACHROMATIC_SATURATION = 1f;

        /// <summary>
        /// Hsv walks the picker's own numbers, so a gradient lands exactly on the values the user set. Rgb
        /// and Lab are deliberately other spaces, and only those convert. The ends are never round tripped.
        /// </summary>
        public static SeHsv Lerp(SeHsv from, SeHsv to, float amount, PaintBlendSpace space) {
            var t = MathHelper.Clamp(amount, 0f, 1f);

            if (t <= 0f) {
                return from;
            }

            if (t >= 1f) {
                return to;
            }

            if (space == PaintBlendSpace.Hsv) {
                return LerpSeHsv(from, to, t);
            }

            ColorModel start = ((ColorMask)from).ToDisplayColor();
            ColorModel end = ((ColorMask)to).ToDisplayColor();

            return (SeHsv)ColorMask.FromColor(space == PaintBlendSpace.Lab ? LerpLab(start, end, t) : LerpRgb(start, end, t));
        }

        /// <summary>
        /// Takes the hue the short way round.
        /// </summary>
        private static SeHsv LerpSeHsv(SeHsv from, SeHsv to, float t) {
            var startHue = from.H;
            var endHue = to.H;

            if (from.S < ACHROMATIC_SATURATION) {
                startHue = endHue;
            } else if (to.S < ACHROMATIC_SATURATION) {
                endHue = startHue;
            }

            var hue = startHue + ShortestHueDelta(startHue, endHue) * t;
            if (hue < 0f) {
                hue += 360f;
            } else if (hue >= 360f) {
                hue -= 360f;
            }

            return new SeHsv(hue, MathHelper.Lerp(from.S, to.S, t), MathHelper.Lerp(from.V, to.V, t));
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
