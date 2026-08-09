using Sisk.BuildColors.Settings.Models.ColorSpace;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using VRageMath;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Services {

    /// <summary>
    ///     Mixes two paint colors. Which space the mix happens in decides what the middle of a gradient looks
    ///     like far more than the ends do: RGB darkens through the middle, HSV keeps the colors saturated by
    ///     travelling around the hue circle, and Lab spaces the steps out the way an eye reads them.
    /// </summary>
    internal static class PaintColorBlend {

        /// <summary>
        ///     Below this saturation a color carries no hue worth blending, so the other end's hue is used
        ///     instead of travelling towards an arbitrary one.
        /// </summary>
        private const float ACHROMATIC_SATURATION = .01f;

        public static ColorModel Lerp(ColorModel from, ColorModel to, float amount, PaintBlendSpace space) {
            var t = MathHelper.Clamp(amount, 0f, 1f);

            if (t <= 0f) {
                return from;
            }

            if (t >= 1f) {
                return to;
            }

            switch (space) {
                case PaintBlendSpace.Hsv:
                    return LerpHsv(from, to, t);

                case PaintBlendSpace.Lab:
                    return LerpLab(from, to, t);

                default:
                    return LerpRgb(from, to, t);
            }
        }

        private static ColorModel LerpRgb(ColorModel from, ColorModel to, float t) {
            return new ColorModel(
                LerpByte(from.R, to.R, t),
                LerpByte(from.G, to.G, t),
                LerpByte(from.B, to.B, t));
        }

        private static ColorModel LerpHsv(ColorModel from, ColorModel to, float t) {
            var start = new RGB(from.R, from.G, from.B).ToHSV();
            var end = new RGB(to.R, to.G, to.B).ToHSV();

            var startHue = start.H;
            var endHue = end.H;

            // A grey end has no hue of its own, so it borrows the other one instead of dragging the blend
            // across the hue circle towards zero.
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
        ///     Signed distance from one hue to another the short way round, so red and violet meet across zero
        ///     instead of sweeping through the whole circle.
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
