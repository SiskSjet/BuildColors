using System;
using System.Linq;
using VRageMath;

namespace Sisk.BuildColors {

    public class ColorSchemeGenerator {

        private static readonly float[,] _presetDefaults = {
            { -0.1f, -0.4f }, // Default
            { 0.5f, 0.5f }, // Pastel
            { 0.3f, 0.6f }, // Soft
            { 0.5f, 0.75f }, // Light
            { 1f, -0.8f }, // Hard
            { 0.1f, 0.5f }, // Pale
        };

        private readonly Random _random = new Random();
        private Color _baseColor;

        public enum Preset {
            Random,
            Pastel,
            Soft,
            Light,
            Hard,
            Pale
        }

        public enum Scheme {
            Analogous,
            Complementary,
            Monochromatic,
            Tetradic,
            Triadic
        }

        public Color BaseColor {
            get { return _baseColor; }
            set { _baseColor = value; }
        }

        public Color ConvertColor(Color color, Preset preset) {
            if (!Enum.IsDefined(typeof(Preset), preset)) {
                preset = Preset.Random;
            }

            var baseSaturation = _presetDefaults[(int)preset, 0];
            var baseBrightness = _presetDefaults[(int)preset, 1];

            var hue = color.GetHue();
            var saturation = baseSaturation > 0 ? baseSaturation : _random.Next((int)(Math.Abs(baseSaturation) * 100), 100) / 100f;
            var brightness = baseBrightness > 0 ? baseBrightness : _random.Next((int)(Math.Abs(baseBrightness) * 100), 100) / 100f;

            return ColorFromHSB(hue, saturation, brightness);
        }

        public Color[] Generate(Color baseColor = default(Color), Scheme scheme = Scheme.Complementary, Preset preset = Preset.Random) {
            // generate a base color if not specified
            if (baseColor == default(Color)) {
                baseColor = GetRandomColor();
            }

            _baseColor = baseColor;
            var adjustedColor = ConvertColor(baseColor, preset);

            //return colors;
            switch (scheme) {
                case Scheme.Analogous:
                    return GenerateAnalogousScheme(adjustedColor);

                case Scheme.Complementary:
                    return GenerateComplementaryScheme(adjustedColor);

                case Scheme.Monochromatic:
                    return GenerateMonochromaticScheme(adjustedColor);

                case Scheme.Tetradic:
                    return GenerateTetradicScheme(adjustedColor);

                case Scheme.Triadic:
                    return GenerateTriadicScheme(adjustedColor);

                default:
                    throw new Exception("Invalid scheme!");
            }
        }

        public Color[] GenerateAnalogousScheme(Color baseColor) {
            var colors = new Color[14];

            var analogousColors = GetAnalogousColors(baseColor, 7, 1.5f);

            Array.Copy(analogousColors, 0, colors, 0, analogousColors.Length);
            var shades = analogousColors.Select(x => GetMonochromaticColors(x, 2, .4f)[1]).ToArray();
            Array.Copy(shades, 0, colors, 7, shades.Length);

            return colors;
        }

        public Color[] GenerateComplementaryScheme(Color baseColor) {
            var colors = new Color[14];

            colors[0] = baseColor;
            var complementary = GetComplementaryColor(baseColor);
            Array.Copy(GetMonochromaticColors(baseColor, 3, 0.2f), 0, colors, 1, 3);
            Array.Copy(GetMonochromaticColors(complementary, 3, 0.2f), 0, colors, 4, 3);

            // Generate split-complementary colors
            var splitComp = GetSplitComplementaryColors(baseColor, 0.5f);
            Array.Copy(GetMonochromaticColors(splitComp[0], 4, 0.2f), 0, colors, 7, 4);
            Array.Copy(GetMonochromaticColors(splitComp[1], 4, 0.2f), 0, colors, 11, 2);
            colors[13] = complementary;

            return colors;
        }

        public Color[] GenerateMonochromaticScheme(Color baseColor) {
            var colors = new Color[14];

            colors[0] = baseColor;
            Array.Copy(GetMonochromaticColors(baseColor, 5, 0.2f), 0, colors, 1, 5);

            // Generate split-complementary colors
            var splitComp = GetSplitComplementaryColors(baseColor, 0.5f);
            Array.Copy(GetMonochromaticColors(splitComp[0], 4, 0.2f), 0, colors, 6, 4);
            Array.Copy(GetMonochromaticColors(splitComp[1], 4, 0.2f), 0, colors, 10, 4);

            return colors;
        }

        public Color[] GenerateTetradicScheme(Color baseColor) {
            var colors = new Color[14];

            var tetradic = GetTetradicColors(baseColor, 1);

            colors[0] = baseColor;
            Array.Copy(GetMonochromaticColors(baseColor, 3, 0.2f), 0, colors, 1, 3);
            Array.Copy(GetMonochromaticColors(tetradic[0], 3, 0.2f), 0, colors, 4, 3);
            Array.Copy(GetMonochromaticColors(tetradic[1], 3, 0.2f), 0, colors, 7, 3);
            colors[10] = GetComplementaryColor(baseColor);
            Array.Copy(GetMonochromaticColors(tetradic[2], 3, 0.2f), 0, colors, 11, 3);

            return colors;
        }

        public Color[] GenerateTriadicScheme(Color baseColor) {
            var colors = new Color[14];

            var triadic = GetTriadicColors(baseColor, 1);

            colors[0] = baseColor;
            Array.Copy(GetMonochromaticColors(triadic[0], 4, 0.2f), 0, colors, 1, 4);
            Array.Copy(GetMonochromaticColors(triadic[1], 4, 0.2f), 0, colors, 5, 4);
            Array.Copy(GetAnalogousColors(triadic[0], 2, 0.5f), 0, colors, 9, 2);
            Array.Copy(GetAnalogousColors(triadic[1], 2, -0.5f), 0, colors, 11, 2);
            colors[13] = GetMonochromaticColors(GetComplementaryColor(baseColor), 1, 0.2f)[0];

            return colors;
        }

        private static Color ColorFromHSB(float hue, float saturation, float brightness) {
            var chroma = brightness * saturation;
            var hue2 = hue / 60;
            var x = chroma * (1 - Math.Abs(hue2 % 2 - 1));
            float r1, g1, b1;

            if (hue2 < 1) {
                r1 = chroma;
                g1 = x;
                b1 = 0;
            } else if (hue2 < 2) {
                r1 = x;
                g1 = chroma;
                b1 = 0;
            } else if (hue2 < 3) {
                r1 = 0;
                g1 = chroma;
                b1 = x;
            } else if (hue2 < 4) {
                r1 = 0;
                g1 = x;
                b1 = chroma;
            } else if (hue2 < 5) {
                r1 = x;
                g1 = 0;
                b1 = chroma;
            } else {
                r1 = chroma;
                g1 = 0;
                b1 = x;
            }

            var m = brightness - chroma;
            var r = (int)((r1 + m) * 255);
            var g = (int)((g1 + m) * 255);
            var b = (int)((b1 + m) * 255);

            return new Color(r, g, b);
        }

        /// <summary>
        /// Generates an array of Color objects with similar hue and saturation to the base color.
        /// </summary>
        /// <param name="baseColor">The base Color object to generate analogous colors from.</param>
        /// <param name="colorCount">The number of analogous colors to generate.</param>
        /// <param name="degreeFactor">An optional float value to adjust the degree of separation between each analogous color.</param>
        /// <returns>An array of Color objects with similar hue and saturation to the base color.</returns>
        private Color[] GetAnalogousColors(Color baseColor, int colorCount, float degreeFactor = 1) {
            var colors = new Color[colorCount];

            // generate colors with similar hue and saturation
            var hue = baseColor.GetHue();
            var saturation = baseColor.GetSaturation();
            var brightness = baseColor.GetBrightness();

            var degree = 30 * degreeFactor;

            for (var i = 0; i < colorCount; i++) {
                var offset = (i + 1) * degree;
                var h = (hue + offset) % 360;
                colors[i] = ColorFromHSB(h, saturation, brightness);
            }

            return colors;
        }

        /// <summary>
        /// Generates a Color object that is complementary to the base color.
        /// </summary>
        /// <param name="baseColor">The base Color object to generate a complementary color from.</param>
        /// <returns>A Color object that is complementary to the base color.</returns>
        private Color GetComplementaryColor(Color baseColor) {
            // generate complementary colors
            var hue = baseColor.GetHue();
            var saturation = baseColor.GetSaturation();
            var brightness = baseColor.GetBrightness();

            return ColorFromHSB((hue + 180) % 360, saturation, brightness);
        }

        /// <summary>
        /// Generates an array of Color objects with varying brightness levels based on the base color.
        /// </summary>
        /// <param name="baseColor">The base Color object to generate monochromatic colors from.</param>
        /// <param name="colorCount">The number of monochromatic colors to generate.</param>
        /// <param name="shadeFactor">An optional float value to adjust the brightness level of each color.</param>
        /// <returns>An array of Color objects with varying brightness levels based on the base color.</returns>
        private Color[] GetMonochromaticColors(Color baseColor, int colorCount, float shadeFactor = 0.3f) {
            var colors = new Color[colorCount];

            // generate shades or tints of the base color
            var hue = baseColor.GetHue();
            var saturation = baseColor.GetSaturation();
            var brightness = baseColor.GetBrightness();

            for (var i = 0; i < colorCount; i++) {
                var b = Math.Max(0, Math.Min(1, brightness - i * shadeFactor));
                colors[i] = ColorFromHSB(hue, saturation, b);
            }

            return colors;
        }

        private Color GetRandomColor() {
            var random = new Random();

            // generate a random color
            var hue = (float)random.NextDouble() * 360;
            var saturation = (float)random.NextDouble();
            var brightness = (float)random.NextDouble();

            return ColorFromHSB(hue, saturation, brightness);
        }

        /// <summary>
        /// Generates an array of two Color objects that are split complementary to the base color.
        /// </summary>
        /// <param name="baseColor">The base Color object to generate split complementary colors from.</param>
        /// <param name="distanceFactor">An optional float value to adjust the distance between each split complementary color.</param>
        /// <returns>An array of two Color objects that are split complementary to the base color.</returns>
        private Color[] GetSplitComplementaryColors(Color baseColor, float distanceFactor = 1) {
            var colors = new Color[2];

            // generate split complementary colors
            var hue = baseColor.GetHue();
            var saturation = baseColor.GetSaturation();
            var brightness = baseColor.GetBrightness();

            var distance = 150 * distanceFactor;

            colors[0] = ColorFromHSB((hue + 180 + distance) % 360, saturation, brightness);
            colors[1] = ColorFromHSB((hue + 180 - distance) % 360, saturation, brightness);

            return colors;
        }

        /// <summary>
        /// Generates an array of three Color objects that are tetradic to the base color.
        /// </summary>
        /// <param name="baseColor">The base Color object to generate tetradic colors from.</param>
        /// <param name="degreeFactor">An optional float value to adjust the angle between each tetradic color.</param>
        /// <returns>An array of three Color objects that are tetradic to the base color.</returns>

        private Color[] GetTetradicColors(Color baseColor, float degreeFactor = 1) {
            var colors = new Color[3];

            // generate tetradic colors
            var hue = baseColor.GetHue();
            var saturation = baseColor.GetSaturation();
            var brightness = baseColor.GetBrightness();

            var degree = 45 * degreeFactor;

            colors[0] = ColorFromHSB((hue + degree) % 360, saturation, brightness);
            colors[1] = ColorFromHSB((hue + 180) % 360, saturation, brightness);
            colors[2] = ColorFromHSB((hue + 180 - degree) % 360, saturation, brightness);

            return colors;
        }

        /// <summary>
        /// Generates an array of two Color objects that are triadic to the base color.
        /// </summary>
        /// <param name="baseColor">The base Color object to generate triadic colors from.</param>
        /// <param name="distanceFactor">An optional float value to adjust the distance between each triadic color.</param>
        /// <returns>An array of two Color objects that are triadic to the base color.</returns>
        private Color[] GetTriadicColors(Color baseColor, float distanceFactor = 1) {
            var colors = new Color[2];

            // generate triadic colors
            var hue = baseColor.GetHue();
            var saturation = baseColor.GetSaturation();
            var brightness = baseColor.GetBrightness();

            var distance = 60 * distanceFactor;

            colors[0] = ColorFromHSB((hue + 180 - distance) % 360, saturation, brightness);
            colors[1] = ColorFromHSB((hue + 180 + distance) % 360, saturation, brightness);

            return colors;
        }
    }
}