using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;
using VRageMath;

namespace Sisk.BuildColors.Services {

    /// <summary>
    ///     What a positional source needs to know about the grid it is painting. Bounds and orientation are
    ///     the same for every block on a grid, so they are read once before the block walk instead of per
    ///     block.
    /// </summary>
    internal struct GridPaintContext {

        /// <summary>
        ///     Center of the grid in block coordinates.
        /// </summary>
        public Vector3 Center;

        /// <summary>
        ///     Half the extent of the grid per axis, never below half a block so a single block grid still
        ///     normalizes instead of dividing by zero.
        /// </summary>
        public Vector3 Extent;

        /// <summary>
        ///     Index of the grid axis the build is longest on: 0 for X, 1 for Y, 2 for Z.
        /// </summary>
        public int LongestAxis;

        public Vector3 Min;

        /// <summary>
        ///     Full extent per axis.
        /// </summary>
        public Vector3 Size;

        /// <summary>
        ///     Distance from the center to the far corner, used to normalize radial sources.
        /// </summary>
        public float Radius;

        /// <summary>
        ///     Direction world up points in, expressed in block coordinates. A gradient along this axis keeps
        ///     the lighter top a ship was painted with no matter which way its first block was placed.
        /// </summary>
        public Vector3 UpAxis;

        /// <summary>
        ///     Half the extent of the grid measured along <see cref="UpAxis" />.
        /// </summary>
        public float UpExtent;

        public static GridPaintContext Build(IMyCubeGrid grid) {
            var min = new Vector3(grid.Min);
            var max = new Vector3(grid.Max);

            var size = Vector3.Max(max - min, Vector3.One);
            var extent = Vector3.Max(size * .5f, new Vector3(.5f));
            var upAxis = ResolveUpAxis(grid);
            var upExtent = Vector3.Dot(Vector3.Abs(upAxis), extent);

            return new GridPaintContext {
                Min = min,
                Size = size,
                LongestAxis = size.X >= size.Y && size.X >= size.Z ? 0 : size.Y >= size.Z ? 1 : 2,
                Center = (min + max) * .5f,
                Extent = extent,
                Radius = Math.Max(extent.Length(), .5f),
                UpAxis = upAxis,
                UpExtent = Math.Max(upExtent, .5f)
            };
        }

        /// <summary>
        ///     Turns world up into a direction in block coordinates. Natural gravity is what "up" means to
        ///     anyone looking at a ship, so it wins wherever there is any; in space there is no shared up and
        ///     the grid's own up is the only answer that stays put while the grid moves.
        /// </summary>
        private static Vector3 ResolveUpAxis(IMyCubeGrid grid) {
            var worldMatrix = grid.WorldMatrix;
            var gravity = MyAPIGateway.GravityProviderSystem != null
                ? MyAPIGateway.GravityProviderSystem.CalculateNaturalGravityInPoint(worldMatrix.Translation)
                : Vector3.Zero;

            if (gravity.LengthSquared() < .0001f) {
                return Vector3.Up;
            }

            var worldUp = Vector3D.Normalize(-(Vector3D)gravity);
            var localUp = Vector3D.TransformNormal(worldUp, MatrixD.Transpose(worldMatrix));

            return Vector3.Normalize((Vector3)localUp);
        }
    }
}
