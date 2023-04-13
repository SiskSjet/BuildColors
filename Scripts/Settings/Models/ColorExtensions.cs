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
    }
}