using System;
using VRage.Game;
using VRageMath;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable InconsistentNaming

namespace Sisk.BuildColors.Settings.Models {
    public static class ColorExtensions {
        /// <summary>
        ///     Converts SE's color mask to hsv.
        /// </summary>
        /// <param name="colorMask">The color mask to convert.</param>
        /// <returns>Returns an <see cref="Vector3" /> with hsv values</returns>
        public static Vector3 ColorMaskToHSV(this Vector3 colorMask) {
            var h = MathHelper.Clamp(colorMask.X, 0f, 1f);
            var s = MathHelper.Clamp(colorMask.Y + MyColorPickerConstants.SATURATION_DELTA, 0f, 1f);
            var v = MathHelper.Clamp(colorMask.Z + MyColorPickerConstants.VALUE_DELTA - MyColorPickerConstants.VALUE_COLORIZE_DELTA, 0f, 1f);
            return new Vector3(h, s, v);
        }

        /// <summary>
        ///     Converts a hsv <see cref="Vector3" /> to SE's color mask.
        /// </summary>
        /// <param name="hsv">The hsv vector to convert.</param>
        /// <returns>Returns an <see cref="Vector3" /> color mask.</returns>
        public static Vector3 HSVToColorMask(this Vector3 hsv) {
            return new Vector3(MathHelper.Clamp(hsv.X, 0f, 1f), MathHelper.Clamp(hsv.Y - MyColorPickerConstants.SATURATION_DELTA, -1f, 1f), MathHelper.Clamp(hsv.Z - MyColorPickerConstants.VALUE_DELTA + MyColorPickerConstants.VALUE_COLORIZE_DELTA, -1f, 1f));
        }

        //    // `rgbToHsl`
        //    // Converts an RGB color value to HSL.
        //    // *Assumes:* r, g, and b are contained in [0, 255] or [0, 1]
        //    // *Returns:* { h, s, l } in [0,1]
        //    public static Color ToHSL(this Color color) {
        //        float red = color.R;
        //        float green = color.G;
        //        float blue = color.B;
        //        var min = 0f;
        //        var max = 0f;
        //        var hue = 0f;
        //        var saturation = 0f;
        //        var lightness = 0f;
        //        float d;

        //        red /= 255;
        //        green /= 255;
        //        blue /= 255;

        //        max = Math.Max(red, Math.Max(green, blue));
        //        min = Math.Min(red, Math.Min(green, blue));
        //        lightness = (max + min) / 2;

        //        if (max == min) {
        //            hue = saturation = 0;
        //        } else {
        //            d = max - min;
        //            saturation = lightness > 0.5 ? d / (2 - max - min) : d / (max + min);

        //            if (max == red) {
        //                hue = (green - blue) / d + (green < blue ? 6 : 0);
        //            } else if (max == green) {
        //                hue = (blue - red) / d + 2;
        //            } else if (max == blue) {
        //                hue = (red - green) / d + 4;
        //            }

        //            hue /= 6;
        //        }

        //        return [
        //            Math.floor(hue * 360),
        //            Utils.round((saturation * 100), 1),
        //            Utils.round((lightness * 100), 1)
        //        ];
        //    }

        //    public static void RgbToHls(int r, int g, int b, out double h, out double l, out double s) {
        //        // Convert RGB to a 0.0 to 1.0 range.
        //        var double_r = r / 255.0;
        //        var double_g = g / 255.0;
        //        var double_b = b / 255.0;

        //        // Get the maximum and minimum RGB components.
        //        var max = double_r;
        //        if (max < double_g) {
        //            max = double_g;
        //        }

        //        if (max < double_b) {
        //            max = double_b;
        //        }

        //        var min = double_r;
        //        if (min > double_g) {
        //            min = double_g;
        //        }

        //        if (min > double_b) {
        //            min = double_b;
        //        }

        //        var diff = max - min;
        //        l = (max + min) / 2;
        //        if (Math.Abs(diff) < 0.00001) {
        //            s = 0;
        //            h = 0;  // H is really undefined.
        //        } else {
        //            if (l <= 0.5) {
        //                s = diff / (max + min);
        //            } else {
        //                s = diff / (2 - max - min);
        //            }

        //            var r_dist = (max - double_r) / diff;
        //            var g_dist = (max - double_g) / diff;
        //            var b_dist = (max - double_b) / diff;

        //            if (double_r == max) {
        //                h = b_dist - g_dist;
        //            } else if (double_g == max) {
        //                h = 2 + r_dist - b_dist;
        //            } else {
        //                h = 4 + g_dist - r_dist;
        //            }

        //            h = h * 60;
        //            if (h < 0) {
        //                h += 360;
        //            }
        //        }
        //    }


        //    public static VRageMath.Color Complement(this VRageMath.Color color) {
        //        var hsl = color.ToHsl();
        //        hsl.h = (hsl.h + 180) % 360;
        //        return tinycolor(hsl);
        //    }
    }

    public struct HSL {
        public double Hue { get; set; }
        public double Saturation { get; set; }
        public double Lightness { get; set; }
    }
}
