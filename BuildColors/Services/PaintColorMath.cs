using System;
using VRageMath;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Color comparison for paint jobs.
    /// </summary>
    internal static class PaintColorMath {
        /// <summary>
        /// Per component tolerance.
        /// </summary>
        private const float MASK_TOLERANCE = .01f;

        public static bool MaskEquals(Vector3 actual, Vector3 expected) {
            var hueDelta = Math.Abs(actual.X - expected.X);

            if (hueDelta > .5f) {
                hueDelta = 1f - hueDelta;
            }

            return hueDelta <= MASK_TOLERANCE
                && Math.Abs(actual.Y - expected.Y) <= MASK_TOLERANCE
                && Math.Abs(actual.Z - expected.Z) <= MASK_TOLERANCE;
        }
    }
}
