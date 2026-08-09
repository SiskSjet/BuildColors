using ProtoBuf;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public enum PaintSourceType {
        [ProtoEnum]
        Solid = 0,
        [ProtoEnum]
        Gradient = 1,
        [ProtoEnum]
        Camo = 2,
        [ProtoEnum]
        Scatter = 3,
        [ProtoEnum]
        Pattern = 4
    }

    /// <summary>
    ///     Direction a positional source reads its value along. The grid axes are the ones a build is laid
    ///     out on, while <see cref="WorldUp" /> follows gravity so a hull keeps its lighter top no matter how
    ///     the grid was started.
    /// </summary>
    [ProtoContract]
    public enum PaintSourceAxis {
        [ProtoEnum]
        GridX = 0,
        [ProtoEnum]
        GridY = 1,
        [ProtoEnum]
        GridZ = 2,
        [ProtoEnum]
        WorldUp = 3,
        [ProtoEnum]
        Radial = 4,

        /// <summary>
        ///     Whichever grid axis the build is longest on. A gradient "along the ship" is what most people
        ///     mean, and which of the three axes that is depends on how the build was started.
        /// </summary>
        [ProtoEnum]
        Longest = 5
    }

    /// <summary>
    ///     What the two ends of a gradient are pinned to.
    /// </summary>
    [ProtoContract]
    public enum PaintGradientFit {

        /// <summary>
        ///     The ends sit on the outermost blocks the rule actually matches, so both end colors always show
        ///     up. Without this a rule that only covers part of a build - armor on a ship whose nose and tail
        ///     are thrusters, say - never reaches the far end of the gradient and looks like it is missing a
        ///     color.
        /// </summary>
        [ProtoEnum]
        MatchedBlocks = 0,

        /// <summary>
        ///     The ends sit on the bounds of the whole grid, so several rules painting different parts of one
        ///     build share a single gradient.
        /// </summary>
        [ProtoEnum]
        GridBounds = 1
    }

    /// <summary>
    ///     Space two colors are mixed in. Mask space is never used for mixing: the game stores colors as an
    ///     HSV mask with offsets baked in, and interpolating those offsets bends the result away from both
    ///     ends of the blend.
    /// </summary>
    [ProtoContract]
    public enum PaintBlendSpace {
        [ProtoEnum]
        Hsv = 0,
        [ProtoEnum]
        Lab = 1,
        [ProtoEnum]
        Rgb = 2
    }

    [ProtoContract]
    public enum PaintPatternShape {
        [ProtoEnum]
        Stripes = 0,
        [ProtoEnum]
        Checker = 1
    }
}
