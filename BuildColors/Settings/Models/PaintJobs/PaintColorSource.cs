using ProtoBuf;
using System.Collections.Generic;
using System.Xml.Serialization;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    /// <summary>
    /// Where the color and skin of a matched block come from.
    /// </summary>
    [ProtoContract]
    public class PaintColorSource {
        /// <summary>
        /// Widest patch a camo source is allowed, in blocks.
        /// </summary>
        public const float MAX_SCALE = 200f;

        public const int MAX_STEPS = 64;
        public const float MIN_SCALE = .5f;

        public PaintColorSource() {
        }

        [ProtoMember(1)]
        [XmlAttribute("type")]
        public PaintSourceType Type { get; set; } = PaintSourceType.Solid;

        /// <summary>
        /// Axis the source runs along.
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute("axis")]
        public PaintSourceAxis Axis { get; set; } = PaintSourceAxis.Longest;

        [ProtoMember(3)]
        [XmlAttribute("blend")]
        public PaintBlendSpace Blend { get; set; } = PaintBlendSpace.Lab;

        /// <summary>
        /// Number of bands a gradient is snapped to.
        /// </summary>
        [ProtoMember(4)]
        [XmlAttribute("steps")]
        public int Steps { get; set; } = 8;

        /// <summary>
        /// Rough width of a camo patch in blocks.
        /// </summary>
        [ProtoMember(5)]
        [XmlAttribute("scale")]
        public float Scale { get; set; } = 4f;

        /// <summary>
        /// Width of a band or a checker cell in blocks.
        /// </summary>
        [ProtoMember(6)]
        [XmlAttribute("period")]
        public int Period { get; set; } = 3;

        [ProtoMember(7)]
        [XmlAttribute("shape")]
        public PaintPatternShape Shape { get; set; } = PaintPatternShape.Stripes;

        /// <summary>
        /// Varies the pattern without changing anything else about it.
        /// </summary>
        [ProtoMember(8)]
        [XmlAttribute("seed")]
        public int Seed { get; set; }

        [ProtoMember(9)]
        [XmlAttribute("reverse")]
        public bool Reverse { get; set; }

        [ProtoMember(12)]
        [XmlAttribute("fit")]
        public PaintGradientFit Fit { get; set; } = PaintGradientFit.MatchedBlocks;

        [ProtoMember(10)]
        [XmlArray(Order = 10)]
        [XmlArrayItem("Stop")]
        public List<PaintColorStop> Stops { get; set; }

        [ProtoMember(11)]
        [XmlArray(Order = 11)]
        [XmlArrayItem("Entry")]
        public List<PaintPaletteEntry> Palette { get; set; }

        /// <summary>
        /// True when the source reads a palette rather than gradient stops.
        /// </summary>
        public bool UsesPalette {
            get { return Type == PaintSourceType.Camo || Type == PaintSourceType.Scatter || Type == PaintSourceType.Pattern; }
        }

        public bool UsesStops {
            get { return Type == PaintSourceType.Gradient; }
        }

        public PaintColorSource Clone() {
            var clone = new PaintColorSource {
                Type = Type,
                Axis = Axis,
                Blend = Blend,
                Steps = Steps,
                Scale = Scale,
                Period = Period,
                Shape = Shape,
                Seed = Seed,
                Reverse = Reverse,
                Fit = Fit
            };

            if (Stops != null) {
                clone.Stops = new List<PaintColorStop>();
                foreach (var stop in Stops) {
                    clone.Stops.Add(stop.Clone());
                }
            }

            if (Palette != null) {
                clone.Palette = new List<PaintPaletteEntry>();
                foreach (var entry in Palette) {
                    clone.Palette.Add(entry.Clone());
                }
            }

            return clone;
        }

        /// <summary>
        /// Fills the list a source of this type reads, so a type freshly switched to always has something to show.
        /// </summary>
        public void EnsureEntries(ColorModel seedColor) {
            if (UsesStops) {
                if (Stops == null) {
                    Stops = new List<PaintColorStop>();
                }

                while (Stops.Count < 2) {
                    Stops.Add(new PaintColorStop(seedColor, Stops.Count));
                }

                return;
            }

            if (!UsesPalette) {
                return;
            }

            if (Palette == null) {
                Palette = new List<PaintPaletteEntry>();
            }

            while (Palette.Count < 2) {
                Palette.Add(new PaintPaletteEntry(seedColor));
            }
        }
    }
}
