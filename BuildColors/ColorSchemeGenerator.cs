using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.ColorSpace;
using System;
using System.Collections.Generic;
using VRageMath;

namespace Sisk.BuildColors {

    /// <summary>
    /// Builds a fourteen slot build palette from a base color and a color scheme.
    /// </summary>
    public class ColorSchemeGenerator {
        /// <summary>
        /// Slots of the first row, which carries the primary ramp.
        /// </summary>
        public const int RAMP_SLOTS = 7;

        /// <summary>
        /// Perceptual distance two slots must clear before they read as different colors.
        /// </summary>
        public const float DEFAULT_MIN_DISTANCE = 7f;

        public const int DEFAULT_GREY_COUNT = 3;
        public const int MAX_GREY_COUNT = 6;

        /// <summary>
        /// Passes the spreading step is allowed before it gives up on a palette that cannot be separated.
        /// </summary>
        private const int SPREAD_PASSES = 8;

        /// <summary>
        /// Lightness a palette covers when it is grown from a color of its own choosing.
        /// </summary>
        private const float LIGHTNESS_SPAN = .74f;

        private const float MIN_LIGHTNESS = .03f;
        private const float MAX_LIGHTNESS = .97f;

        /// <summary>
        /// Least lightness and saturation a palette keeps at either extreme.
        /// </summary>
        private const float MIN_LIGHTNESS_SPREAD = .18f;

        private const float MIN_SATURATION_SPREAD = .12f;
        private const float SATURATION_BELOW = .3f;
        private const float SATURATION_ABOVE = .25f;

        /// <summary>
        /// Saturation and lightness envelope, plus hue band, of each preset.
        /// </summary>
        private static readonly float[,] _presetRanges = {
            { .25f, .95f, .14f, .88f, 0f, 360f },
            { .25f, .45f, .70f, .92f, 0f, 360f },
            { .30f, .55f, .45f, .80f, 0f, 360f },
            { .40f, .80f, .55f, .92f, 0f, 360f },
            { .80f, 1.0f, .35f, .70f, 0f, 360f },
            { .10f, .30f, .60f, .88f, 0f, 360f },
            { .75f, 1.0f, .40f, .65f, 0f, 360f },
            { .15f, .40f, .30f, .65f, 0f, 360f },
            { .40f, .85f, .25f, .80f, 0f, 60f },
            { .35f, .80f, .25f, .80f, 180f, 270f },
            { .30f, .70f, .08f, .40f, 0f, 360f },
            { .30f, .70f, .60f, .95f, 0f, 360f }
        };

        private readonly Random _seedSource = new Random();

        public enum Preset {
            None,
            Pastel,
            Soft,
            Light,
            Hard,
            Pale,
            Vibrant,
            Muted,
            Warm,
            Cool,
            Dark,
            Lighter
        }

        public enum Scheme {
            Default,
            Analogous,
            Complementary,
            SplitComplementary,
            Triadic,
            Tetradic,
            Square,
            Monochromatic,
            HullAndAccent
        }

        /// <summary>
        /// What a palette is built from.
        /// </summary>
        public class Options {
            public Scheme Scheme = Scheme.Default;
            public Preset Preset = Preset.None;

            /// <summary>
            /// The color the scheme is grown from, or null to roll one from the seed.
            /// </summary>
            public HSL? BaseColor;

            /// <summary>
            /// The seed to roll from, or null for a fresh one.
            /// </summary>
            public int? Seed;

            public int GreyCount = DEFAULT_GREY_COUNT;
            public float MinDistance = DEFAULT_MIN_DISTANCE;

            /// <summary>
            /// Slots kept exactly as they are, by index.
            /// </summary>
            public bool[] Locked;

            /// <summary>
            /// The palette the locked slots are taken from.
            /// </summary>
            public ColorMask[] Current;

            public Options Clone() {
                return new Options {
                    Scheme = Scheme,
                    Preset = Preset,
                    BaseColor = BaseColor,
                    Seed = Seed,
                    GreyCount = GreyCount,
                    MinDistance = MinDistance,
                    Locked = Locked,
                    Current = Current,
                };
            }
        }

        /// <summary>
        /// A generated palette and everything needed to roll it again.
        /// </summary>
        public struct Result {
            public ColorMask[] Masks;
            public int Seed;
            public HSL BaseColor;

            public List<Vector3> ToSlots() {
                var slots = new List<Vector3>(ColorSet.SLOTS);

                for (var i = 0; i < ColorSet.SLOTS; i++) {
                    slots.Add(Masks != null && i < Masks.Length ? (Vector3)Masks[i] : Vector3.Zero);
                }

                return slots;
            }
        }

        /// <summary>
        /// The color the last palette was grown from.
        /// </summary>
        public HSL BaseColor { get; private set; }

        /// <summary>
        /// The slot of a palette worth growing a scheme from, which is its most colorful one.
        /// </summary>
        public static int GetDominantIndex(ColorMask[] palette) {
            var best = 0;
            var bestScore = -1f;

            if (palette == null) {
                return best;
            }

            for (var i = 0; i < palette.Length; i++) {
                var color = ToHSL(palette[i]);

                var score = color.S * (1f - Math.Abs(color.L - .5f) * 1.4f);

                if (score > bestScore) {
                    bestScore = score;
                    best = i;
                }
            }

            return best;
        }

        public static HSL ToHSL(ColorMask mask) {
            var hsv = ((Vector3)mask).ColorMaskToHSV();

            return new HSV(hsv.X * 360f, hsv.Y, hsv.Z).ToHSL();
        }

        public static ColorMask ToMask(HSL color) {
            var hsv = color.ToHSV();
            var vector = new Vector3(Wrap(hsv.H) / 360f, MathHelper.Clamp(hsv.S, 0f, 1f), MathHelper.Clamp(hsv.V, 0f, 1f));

            return vector.HSVToColorMask();
        }

        /// <summary>
        /// Builds a palette.
        /// </summary>
        public Result Generate(Options options = null) {
            options = options ?? new Options();

            var seed = options.Seed ?? NewSeed();
            var random = new Random(seed);
            var baseColor = options.BaseColor ?? RollBaseColor(random);

            BaseColor = baseColor;

            var envelope = GetEnvelope(options.Preset, baseColor);
            var hues = GetSchemeHues(baseColor.H, options.Scheme);
            var colors = BuildPalette(hues, envelope, options, random);

            ApplyHueBand(colors, envelope);
            ApplyLocks(colors, options);
            Spread(colors, envelope, options);

            return new Result {
                Masks = ToMasks(colors, options),
                Seed = seed,
                BaseColor = baseColor,
            };
        }

        public int NewSeed() {
            return _seedSource.Next(1, int.MaxValue);
        }

        /// <summary>
        /// Hues of the scheme, in the order the palette should use them.
        /// </summary>
        private static float[] GetSchemeHues(float hue, Scheme scheme) {
            switch (scheme) {
                case Scheme.Analogous:
                    return new[] { hue, Wrap(hue + 30f), Wrap(hue - 30f), Wrap(hue + 60f), Wrap(hue - 60f) };

                case Scheme.Complementary:
                    return new[] { hue, Wrap(hue + 180f) };

                case Scheme.SplitComplementary:
                    return new[] { hue, Wrap(hue + 150f), Wrap(hue + 210f) };

                case Scheme.Triadic:
                    return new[] { hue, Wrap(hue + 120f), Wrap(hue + 240f) };

                case Scheme.Tetradic:
                    return new[] { hue, Wrap(hue + 60f), Wrap(hue + 180f), Wrap(hue + 240f) };

                case Scheme.Square:
                    return new[] { hue, Wrap(hue + 90f), Wrap(hue + 180f), Wrap(hue + 270f) };

                case Scheme.Monochromatic:
                    return new[] { hue };

                case Scheme.HullAndAccent:
                    return new[] { hue, Wrap(hue + 180f) };

                default:
                    return new[] { hue, Wrap(hue + 180f), Wrap(hue + 150f), Wrap(hue + 210f) };
            }
        }

        private static float Wrap(float hue) {
            hue %= 360f;

            return hue < 0f ? hue + 360f : hue;
        }

        private static float Lerp(float from, float to, float amount) {
            return from + (to - from) * MathHelper.Clamp(amount, 0f, 1f);
        }

        /// <summary>
        /// The saturation and lightness the palette is built inside.
        /// </summary>
        private static float[] GetEnvelope(Preset preset, HSL baseColor) {
            if (!Enum.IsDefined(typeof(Preset), preset)) {
                preset = Preset.None;
            }

            if (preset != Preset.None) {
                var index = (int)preset;

                return new[] {
                    _presetRanges[index, 0], _presetRanges[index, 1],
                    _presetRanges[index, 2], _presetRanges[index, 3],
                    _presetRanges[index, 4], _presetRanges[index, 5]
                };
            }

            var span = LIGHTNESS_SPAN * (.35f + .65f * (1f - Math.Abs(baseColor.L * 2f - 1f)));

            var minLightness = MathHelper.Clamp(baseColor.L - span * .5f, MIN_LIGHTNESS, MAX_LIGHTNESS);
            var maxLightness = MathHelper.Clamp(baseColor.L + span * .5f, MIN_LIGHTNESS, MAX_LIGHTNESS);

            if (maxLightness - minLightness < MIN_LIGHTNESS_SPREAD) {
                var center = MathHelper.Clamp(baseColor.L, MIN_LIGHTNESS + MIN_LIGHTNESS_SPREAD * .5f, MAX_LIGHTNESS - MIN_LIGHTNESS_SPREAD * .5f);

                minLightness = center - MIN_LIGHTNESS_SPREAD * .5f;
                maxLightness = center + MIN_LIGHTNESS_SPREAD * .5f;
            }

            var minSaturation = MathHelper.Clamp(baseColor.S - SATURATION_BELOW, 0f, 1f);
            var maxSaturation = MathHelper.Clamp(baseColor.S + SATURATION_ABOVE, 0f, 1f);

            if (maxSaturation - minSaturation < MIN_SATURATION_SPREAD) {
                var center = MathHelper.Clamp(baseColor.S, MIN_SATURATION_SPREAD * .5f, 1f - MIN_SATURATION_SPREAD * .5f);

                minSaturation = center - MIN_SATURATION_SPREAD * .5f;
                maxSaturation = center + MIN_SATURATION_SPREAD * .5f;
            }

            return new[] { minSaturation, maxSaturation, minLightness, maxLightness, 0f, 360f };
        }

        /// <summary>
        /// Rolls a base color worth building on: mid lightness and enough saturation to have a hue at all.
        /// </summary>
        private static HSL RollBaseColor(Random random) {
            var hue = (float)random.NextDouble() * 360f;
            var saturation = .45f + (float)random.NextDouble() * .45f;
            var lightness = .35f + (float)random.NextDouble() * .3f;

            return new HSL(hue, saturation, lightness);
        }

        /// <summary>
        /// Lays out the primary ramp, then the accents, then the neutral ramp.
        /// </summary>
        private static HSL[] BuildPalette(float[] hues, float[] envelope, Options options, Random random) {
            var colors = new HSL[ColorSet.SLOTS];

            var minSaturation = envelope[0];
            var maxSaturation = envelope[1];
            var minLightness = envelope[2];
            var maxLightness = envelope[3];

            var greyCount = MathHelper.Clamp(options.GreyCount, 0, MAX_GREY_COUNT);
            var accentCount = ColorSet.SLOTS - RAMP_SLOTS - greyCount;

            var rampSaturation = options.Scheme == Scheme.HullAndAccent
                ? Lerp(minSaturation, maxSaturation, .15f)
                : Lerp(minSaturation, maxSaturation, .55f);

            for (var i = 0; i < RAMP_SLOTS; i++) {
                var t = i / (float)(RAMP_SLOTS - 1);

                var falloff = 1f - .35f * (float)Math.Pow(Math.Abs(t * 2f - 1f), 1.5);

                colors[i] = new HSL(hues[0], MathHelper.Clamp(rampSaturation * falloff, 0f, 1f), Lerp(maxLightness, minLightness, t));
            }

            for (var i = 0; i < accentCount; i++) {
                var index = RAMP_SLOTS + i;
                var t = accentCount > 1 ? i / (float)(accentCount - 1) : .5f;

                float hue;
                float saturation;
                float lightness;

                if (hues.Length > 1) {
                    hue = hues[1 + i % (hues.Length - 1)];
                    saturation = Lerp(minSaturation, maxSaturation, .8f);
                    lightness = Lerp(Lerp(maxLightness, minLightness, .25f), Lerp(maxLightness, minLightness, .75f), t);
                } else {
                    hue = hues[0];
                    saturation = Lerp(minSaturation, maxSaturation, .9f);
                    lightness = Lerp(Lerp(maxLightness, minLightness, .15f), Lerp(maxLightness, minLightness, .85f), t);
                }

                var jitter = ((float)random.NextDouble() * 2f - 1f) * 4f;
                colors[index] = new HSL(Wrap(hue + jitter), MathHelper.Clamp(saturation, 0f, 1f), lightness);
            }

            for (var i = 0; i < greyCount; i++) {
                var index = ColorSet.SLOTS - greyCount + i;
                var t = greyCount > 1 ? i / (float)(greyCount - 1) : .5f;

                colors[index] = new HSL(hues[0], Lerp(.03f, .10f, t), Lerp(minLightness, maxLightness, t));
            }

            return colors;
        }

        /// <summary>
        /// Folds the hues into the band of a preset that has one, such as warm or cool.
        /// </summary>
        private static void ApplyHueBand(HSL[] colors, float[] envelope) {
            var minHue = envelope[4];
            var maxHue = envelope[5];

            if (minHue <= 0f && maxHue >= 360f) {
                return;
            }

            for (var i = 0; i < colors.Length; i++) {
                colors[i] = new HSL(minHue + (maxHue - minHue) * (Wrap(colors[i].H) / 360f), colors[i].S, colors[i].L);
            }
        }

        /// <summary>
        /// Puts the locked slots back, so generating around them leaves them untouched.
        /// </summary>
        private static void ApplyLocks(HSL[] colors, Options options) {
            if (options.Locked == null || options.Current == null) {
                return;
            }

            for (var i = 0; i < colors.Length && i < options.Locked.Length; i++) {
                if (options.Locked[i] && i < options.Current.Length) {
                    colors[i] = ToHSL(options.Current[i]);
                }
            }
        }

        /// <summary>
        /// Pushes slots that read as the same color apart until they clear the distance floor.
        /// </summary>
        private static void Spread(HSL[] colors, float[] envelope, Options options) {
            if (options.MinDistance <= 0f) {
                return;
            }

            var minLightness = envelope[2];
            var maxLightness = envelope[3];
            var locked = options.Locked;

            var room = MathHelper.Clamp((maxLightness - minLightness) / LIGHTNESS_SPAN, .35f, 1f);
            var minDistance = options.MinDistance * room;

            for (var pass = 0; pass < SPREAD_PASSES; pass++) {
                var moved = false;

                for (var i = 0; i < colors.Length; i++) {
                    if (locked != null && i < locked.Length && locked[i]) {
                        continue;
                    }

                    for (var j = 0; j < colors.Length; j++) {
                        if (i == j) {
                            continue;
                        }

                        var distance = (float)colors[i].ToLab().GetDistance(colors[j].ToLab());

                        if (distance >= minDistance) {
                            continue;
                        }

                        var deficit = (minDistance - distance) / minDistance;
                        var direction = colors[i].L >= colors[j].L ? 1f : -1f;
                        var step = direction * deficit * .06f;

                        var lightness = MathHelper.Clamp(colors[i].L + step, minLightness, maxLightness);

                        if (Math.Abs(lightness - colors[i].L) < .001f) {
                            colors[i] = new HSL(Wrap(colors[i].H + direction * deficit * 6f), colors[i].S, colors[i].L);
                        } else {
                            colors[i] = new HSL(colors[i].H, colors[i].S, lightness);
                        }

                        moved = true;
                    }
                }

                if (!moved) {
                    return;
                }
            }
        }

        /// <summary>
        /// Turns the palette into slots, keeping the mask of a locked slot exactly as it was.
        /// </summary>
        private static ColorMask[] ToMasks(HSL[] colors, Options options) {
            var masks = new ColorMask[colors.Length];

            for (var i = 0; i < colors.Length; i++) {
                var isLocked = options.Locked != null
                    && i < options.Locked.Length
                    && options.Locked[i]
                    && options.Current != null
                    && i < options.Current.Length;

                masks[i] = isLocked ? options.Current[i] : ToMask(colors[i]);
            }

            return masks;
        }
    }
}
