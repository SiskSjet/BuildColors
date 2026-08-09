using System;
using VRageMath;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Deterministic noise over block coordinates.
    /// </summary>
    internal static class PaintNoise {
        /// <summary>
        /// Mixes a block coordinate and a seed into a well spread integer.
        /// </summary>
        public static uint Hash(int x, int y, int z, int seed) {
            unchecked {
                var hash = (uint)seed * 374761393u + 2166136261u;

                hash += (uint)x * 668265263u;
                hash ^= hash >> 13;
                hash += (uint)y * 2246822519u;
                hash ^= hash >> 15;
                hash += (uint)z * 3266489917u;
                hash ^= hash >> 16;
                hash *= 668265263u;
                hash ^= hash >> 15;

                return hash;
            }
        }

        /// <summary>
        /// Folds a hash into the 0 to 1 range.
        /// </summary>
        public static float UnitValue(uint hash) {
            return (hash & 0xFFFFFF) / 16777216f;
        }

        public static float UnitValue(int x, int y, int z, int seed) {
            return UnitValue(Hash(x, y, z, seed));
        }

        /// <summary>
        /// Value noise sampled at a block position, returning 0 to 1.
        /// </summary>
        public static float ValueNoise(Vector3 position, float scale, int seed) {
            var cellSize = Math.Max(scale, .01f);
            var scaled = position / cellSize;

            var baseX = (int)Math.Floor(scaled.X);
            var baseY = (int)Math.Floor(scaled.Y);
            var baseZ = (int)Math.Floor(scaled.Z);

            var fractionX = Smooth(scaled.X - baseX);
            var fractionY = Smooth(scaled.Y - baseY);
            var fractionZ = Smooth(scaled.Z - baseZ);

            var x0y0 = MathHelper.Lerp(UnitValue(baseX, baseY, baseZ, seed), UnitValue(baseX + 1, baseY, baseZ, seed), fractionX);
            var x1y0 = MathHelper.Lerp(UnitValue(baseX, baseY + 1, baseZ, seed), UnitValue(baseX + 1, baseY + 1, baseZ, seed), fractionX);
            var x0y1 = MathHelper.Lerp(UnitValue(baseX, baseY, baseZ + 1, seed), UnitValue(baseX + 1, baseY, baseZ + 1, seed), fractionX);
            var x1y1 = MathHelper.Lerp(UnitValue(baseX, baseY + 1, baseZ + 1, seed), UnitValue(baseX + 1, baseY + 1, baseZ + 1, seed), fractionX);

            var front = MathHelper.Lerp(x0y0, x1y0, fractionY);
            var back = MathHelper.Lerp(x0y1, x1y1, fractionY);

            return MathHelper.Clamp(MathHelper.Lerp(front, back, fractionZ), 0f, 1f);
        }

        private static float Smooth(float t) {
            return t * t * (3f - 2f * t);
        }
    }
}
