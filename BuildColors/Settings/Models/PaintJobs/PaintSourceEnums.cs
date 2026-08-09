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
    /// Direction a positional source reads its value along.
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
        /// Whichever grid axis the build is longest on.
        /// </summary>
        [ProtoEnum]
        Longest = 5
    }

    /// <summary>
    /// What the two ends of a gradient are pinned to.
    /// </summary>
    [ProtoContract]
    public enum PaintGradientFit {
        /// <summary>
        /// The ends sit on the outermost blocks the rule actually matches, so both end colors always show up.
        /// </summary>
        [ProtoEnum]
        MatchedBlocks = 0,

        /// <summary>
        /// The ends sit on the bounds of the whole grid.
        /// </summary>
        [ProtoEnum]
        GridBounds = 1
    }

    /// <summary>
    /// Space two colors are mixed in.
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
