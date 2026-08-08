using System;
using VRageMath;

namespace Sisk.BuildColors.Services {

    /// <summary>
    ///     Color comparison for paint jobs. The game keeps block colors as an HSV mask, so comparisons are
    ///     made in mask space; converting every block back to RGB just to compare it costs more and loses
    ///     precision on the way.
    /// </summary>
    internal static class PaintColorMath {

        /// <summary>
        ///     Per component tolerance. Covers the rounding the game applies when a mask is stored and is
        ///     roughly the same width as a single RGB step.
        /// </summary>
        private const float MASK_TOLERANCE = .01f;

        public static bool MaskEquals(Vector3 actual, Vector3 expected) {
            var hueDelta = Math.Abs(actual.X - expected.X);

            // Hue is a circle, so 0.99 and 0.01 are neighbours rather than opposites.
            if (hueDelta > .5f) {
                hueDelta = 1f - hueDelta;
            }

            return hueDelta <= MASK_TOLERANCE
                && Math.Abs(actual.Y - expected.Y) <= MASK_TOLERANCE
                && Math.Abs(actual.Z - expected.Z) <= MASK_TOLERANCE;
        }
    }
}
