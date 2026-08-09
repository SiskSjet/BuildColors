using Sisk.BuildColors.Settings.Models.PaintJobs;
using System;
using System.Collections.Generic;
using VRageMath;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// The color and skin one block ends up with.
    /// </summary>
    internal struct PaintEntry {
        public Vector3 Mask;
        public string SkinId;
    }

    /// <summary>
    /// A rule action turned into something that can answer what a block gets.
    /// </summary>
    internal abstract class CompiledPaintSource {
        public abstract void Evaluate(ref BlockFacts facts, out PaintEntry entry);

        /// <summary>
        /// Starts a fresh look at one grid.
        /// </summary>
        public virtual void BeginGrid() { }

        /// <summary>
        /// Called once for every block this source is going to paint, before any of them is evaluated.
        /// </summary>
        public virtual void Observe(ref BlockFacts facts) { }

        /// <summary>
        /// Closes the measuring pass.
        /// </summary>
        public virtual void EndGrid() { }

        public static CompiledPaintSource Compile(PaintRuleAction action) {
            var solid = new PaintEntry { Mask = action.TargetColor, SkinId = action.TargetSkinId ?? string.Empty };
            var source = action.Source;

            if (source == null || source.Type == PaintSourceType.Solid) {
                return new SolidPaintSource(solid);
            }

            switch (source.Type) {
                case PaintSourceType.Gradient:
                    return CompileGradient(source, solid);

                case PaintSourceType.Camo:
                case PaintSourceType.Scatter:
                case PaintSourceType.Pattern:
                    return CompilePalette(source, solid);

                default:
                    return new SolidPaintSource(solid);
            }
        }

        /// <summary>
        /// Reads the 0 to 1 position of a block along the axis a source runs on.
        /// </summary>
        protected static float ReadAxis(ref BlockFacts facts, PaintSourceAxis axis) {
            switch (axis) {
                case PaintSourceAxis.GridX:
                    return facts.NormalizedPosition.X;

                case PaintSourceAxis.GridZ:
                    return facts.NormalizedPosition.Z;

                case PaintSourceAxis.WorldUp:
                    return facts.NormalizedUp;

                case PaintSourceAxis.Radial:
                    return facts.NormalizedRadial;

                case PaintSourceAxis.Longest:
                    return facts.NormalizedLongest;

                default:
                    return facts.NormalizedPosition.Y;
            }
        }

        /// <summary>
        /// Reads the whole block distance of a block along an axis, which is what a banded pattern counts in.
        /// </summary>
        protected static int ReadAxisCoordinate(ref BlockFacts facts, PaintSourceAxis axis) {
            switch (axis) {
                case PaintSourceAxis.GridX:
                    return facts.Position.X;

                case PaintSourceAxis.GridZ:
                    return facts.Position.Z;

                case PaintSourceAxis.WorldUp:
                    return (int)Math.Floor(facts.UpCoordinate);

                case PaintSourceAxis.Radial:
                    return (int)Math.Floor(facts.RadialCoordinate);

                case PaintSourceAxis.Longest:
                    return facts.LongestPosition;

                default:
                    return facts.Position.Y;
            }
        }

        protected static int FloorDiv(int value, int divisor) {
            var quotient = value / divisor;

            return value % divisor != 0 && (value < 0) != (divisor < 0) ? quotient - 1 : quotient;
        }

        protected static int Wrap(int index, int count) {
            var wrapped = index % count;

            return wrapped < 0 ? wrapped + count : wrapped;
        }

        private static CompiledPaintSource CompileGradient(PaintColorSource source, PaintEntry fallback) {
            var stops = BuildStops(source);
            if (stops.Length == 0) {
                return new SolidPaintSource(fallback);
            }

            if (stops.Length == 1) {
                return new SolidPaintSource(new PaintEntry { Mask = stops[0].Color, SkinId = stops[0].SkinId ?? string.Empty });
            }

            var fit = new AxisFit(source.Fit == PaintGradientFit.MatchedBlocks);
            var gradient = new GradientPaintSource(stops, source.Blend, source.Axis, source.Reverse, fit);

            var steps = source.Steps;
            if (steps > 0) {
                steps = Math.Min(steps, PaintColorSource.MAX_STEPS);

                var bands = new PaintEntry[steps];
                for (var i = 0; i < steps; i++) {
                    bands[i] = gradient.Sample((i + .5f) / steps);
                }

                return new BandedPaintSource(bands, source.Axis, source.Reverse, fit);
            }

            return gradient;
        }

        private static CompiledPaintSource CompilePalette(PaintColorSource source, PaintEntry fallback) {
            var entries = BuildPalette(source);
            if (entries.Length == 0) {
                return new SolidPaintSource(fallback);
            }

            if (entries.Length == 1) {
                return new SolidPaintSource(entries[0]);
            }

            switch (source.Type) {
                case PaintSourceType.Camo:
                    return new CamoPaintSource(entries, source.Scale, source.Seed);

                case PaintSourceType.Scatter:
                    return new ScatterPaintSource(entries, BuildWeights(source, entries.Length), source.Seed);

                default:
                    return new PatternPaintSource(entries, source.Shape, source.Axis, Math.Max(source.Period, 1));
            }
        }

        private static PaintColorStop[] BuildStops(PaintColorSource source) {
            if (source.Stops == null || source.Stops.Count == 0) {
                return new PaintColorStop[0];
            }

            var stops = new List<PaintColorStop>();
            foreach (var stop in source.Stops) {
                if (stop != null) {
                    stops.Add(stop);
                }
            }

            stops.Sort((left, right) => left.Position.CompareTo(right.Position));

            return stops.ToArray();
        }

        private static PaintEntry[] BuildPalette(PaintColorSource source) {
            if (source.Palette == null || source.Palette.Count == 0) {
                return new PaintEntry[0];
            }

            var entries = new List<PaintEntry>();
            foreach (var entry in source.Palette) {
                if (entry != null) {
                    entries.Add(new PaintEntry { Mask = entry.Color, SkinId = entry.SkinId ?? string.Empty });
                }
            }

            return entries.ToArray();
        }

        /// <summary>
        /// Turns the weights into the running total a pick is looked up in.
        /// </summary>
        private static float[] BuildWeights(PaintColorSource source, int count) {
            var cumulative = new float[count];
            var total = 0f;

            for (var i = 0; i < count; i++) {
                var entry = source.Palette[i];
                var weight = entry != null ? entry.Weight : 1f;

                total += weight > 0f ? weight : 0f;
                cumulative[i] = total;
            }

            if (total <= 0f) {
                for (var i = 0; i < count; i++) {
                    cumulative[i] = i + 1f;
                }

                total = count;
            }

            for (var i = 0; i < count; i++) {
                cumulative[i] /= total;
            }

            return cumulative;
        }
    }

    /// <summary>
    /// Stretches the span of axis values a rule's blocks actually cover back out over the full 0 to 1 range.
    /// </summary>
    internal sealed class AxisFit {
        private readonly bool _enabled;

        private float _max;
        private float _min;
        private float _span;

        public AxisFit(bool enabled) {
            _enabled = enabled;
        }

        public void Begin() {
            _min = float.MaxValue;
            _max = float.MinValue;
            _span = 0f;
        }

        public void Observe(float value) {
            if (!_enabled) {
                return;
            }

            if (value < _min) {
                _min = value;
            }

            if (value > _max) {
                _max = value;
            }
        }

        public void End() {
            _span = _enabled && _max > _min ? _max - _min : 0f;
        }

        public float Apply(float value) {
            return _span > 0f ? MathHelper.Clamp((value - _min) / _span, 0f, 1f) : value;
        }
    }

    internal sealed class SolidPaintSource : CompiledPaintSource {
        private readonly PaintEntry _entry;

        public SolidPaintSource(PaintEntry entry) {
            _entry = entry;
        }

        public override void Evaluate(ref BlockFacts facts, out PaintEntry entry) {
            entry = _entry;
        }
    }

    /// <summary>
    /// Blends between stops for every block.
    /// </summary>
    internal sealed class GradientPaintSource : CompiledPaintSource {
        private readonly PaintSourceAxis _axis;
        private readonly PaintBlendSpace _blend;
        private readonly AxisFit _fit;
        private readonly bool _reverse;
        private readonly PaintColorStop[] _stops;

        public GradientPaintSource(PaintColorStop[] stops, PaintBlendSpace blend, PaintSourceAxis axis, bool reverse, AxisFit fit) {
            _stops = stops;
            _blend = blend;
            _axis = axis;
            _reverse = reverse;
            _fit = fit;
        }

        public override void BeginGrid() {
            _fit.Begin();
        }

        public override void Observe(ref BlockFacts facts) {
            _fit.Observe(ReadAxis(ref facts, _axis));
        }

        public override void EndGrid() {
            _fit.End();
        }

        public override void Evaluate(ref BlockFacts facts, out PaintEntry entry) {
            var t = _fit.Apply(ReadAxis(ref facts, _axis));

            entry = Sample(_reverse ? 1f - t : t);
        }

        /// <summary>
        /// Color at a point along the gradient.
        /// </summary>
        public PaintEntry Sample(float position) {
            var t = MathHelper.Clamp(position, 0f, 1f);

            if (t <= _stops[0].Position) {
                return ToEntry(_stops[0]);
            }

            var last = _stops.Length - 1;
            if (t >= _stops[last].Position) {
                return ToEntry(_stops[last]);
            }

            for (var i = 0; i < last; i++) {
                var from = _stops[i];
                var to = _stops[i + 1];

                if (t > to.Position) {
                    continue;
                }

                var span = to.Position - from.Position;
                var local = span > 0f ? (t - from.Position) / span : 0f;
                ColorModel blended = PaintColorBlend.Lerp(from.Color, to.Color, local, _blend);

                return new PaintEntry {
                    Mask = blended,
                    SkinId = (local < .5f ? from.SkinId : to.SkinId) ?? string.Empty
                };
            }

            return ToEntry(_stops[last]);
        }

        private static PaintEntry ToEntry(PaintColorStop stop) {
            return new PaintEntry { Mask = stop.Color, SkinId = stop.SkinId ?? string.Empty };
        }
    }

    /// <summary>
    /// A gradient whose colors were all worked out at compile time, leaving one band lookup per block.
    /// </summary>
    internal sealed class BandedPaintSource : CompiledPaintSource {
        private readonly PaintSourceAxis _axis;
        private readonly PaintEntry[] _bands;
        private readonly AxisFit _fit;
        private readonly bool _reverse;

        public BandedPaintSource(PaintEntry[] bands, PaintSourceAxis axis, bool reverse, AxisFit fit) {
            _bands = bands;
            _axis = axis;
            _reverse = reverse;
            _fit = fit;
        }

        public override void BeginGrid() {
            _fit.Begin();
        }

        public override void Observe(ref BlockFacts facts) {
            _fit.Observe(ReadAxis(ref facts, _axis));
        }

        public override void EndGrid() {
            _fit.End();
        }

        public override void Evaluate(ref BlockFacts facts, out PaintEntry entry) {
            var t = _fit.Apply(ReadAxis(ref facts, _axis));
            if (_reverse) {
                t = 1f - t;
            }

            var index = (int)(t * _bands.Length);

            entry = _bands[MathHelper.Clamp(index, 0, _bands.Length - 1)];
        }
    }

    /// <summary>
    /// Picks palette entries in patches.
    /// </summary>
    internal sealed class CamoPaintSource : CompiledPaintSource {
        private readonly PaintEntry[] _entries;
        private readonly float _scale;
        private readonly int _seed;

        public CamoPaintSource(PaintEntry[] entries, float scale, int seed) {
            _entries = entries;
            _scale = MathHelper.Clamp(scale, PaintColorSource.MIN_SCALE, PaintColorSource.MAX_SCALE);
            _seed = seed;
        }

        public override void Evaluate(ref BlockFacts facts, out PaintEntry entry) {
            var noise = PaintNoise.ValueNoise(facts.LocalPosition, _scale, _seed);
            var index = (int)(noise * _entries.Length);

            entry = _entries[MathHelper.Clamp(index, 0, _entries.Length - 1)];
        }
    }

    /// <summary>
    /// Picks a palette entry per block, weighted.
    /// </summary>
    internal sealed class ScatterPaintSource : CompiledPaintSource {
        private readonly float[] _cumulativeWeights;
        private readonly PaintEntry[] _entries;
        private readonly int _seed;

        public ScatterPaintSource(PaintEntry[] entries, float[] cumulativeWeights, int seed) {
            _entries = entries;
            _cumulativeWeights = cumulativeWeights;
            _seed = seed;
        }

        public override void Evaluate(ref BlockFacts facts, out PaintEntry entry) {
            var roll = PaintNoise.UnitValue(facts.Position.X, facts.Position.Y, facts.Position.Z, _seed);

            for (var i = 0; i < _cumulativeWeights.Length; i++) {
                if (roll < _cumulativeWeights[i]) {
                    entry = _entries[i];
                    return;
                }
            }

            entry = _entries[_entries.Length - 1];
        }
    }

    /// <summary>
    /// Repeats the palette in bands or in a checker.
    /// </summary>
    internal sealed class PatternPaintSource : CompiledPaintSource {
        private readonly PaintSourceAxis _axis;
        private readonly PaintEntry[] _entries;
        private readonly int _period;
        private readonly PaintPatternShape _shape;

        public PatternPaintSource(PaintEntry[] entries, PaintPatternShape shape, PaintSourceAxis axis, int period) {
            _entries = entries;
            _shape = shape;
            _axis = axis;
            _period = period;
        }

        public override void Evaluate(ref BlockFacts facts, out PaintEntry entry) {
            int index;

            if (_shape == PaintPatternShape.Checker) {
                index = FloorDiv(facts.Position.X, _period)
                    + FloorDiv(facts.Position.Y, _period)
                    + FloorDiv(facts.Position.Z, _period);
            } else {
                index = FloorDiv(ReadAxisCoordinate(ref facts, _axis), _period);
            }

            entry = _entries[Wrap(index, _entries.Length)];
        }
    }
}
