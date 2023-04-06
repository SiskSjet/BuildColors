using System;
using System.Collections.Generic;
using VRage.Library.Utils;
using VRageMath;
using static Sisk.BuildColors.ColorSchemeGenerator;

namespace Sisk.BuildColors {

    public class ColorSchemeGenerator {

        private static readonly float[,] hsbRanges = {
            { 0.2f, 0.7f, 0.5f, 0.8f }, // Pastel
            { 0.1f, 0.4f, 0.5f, 0.8f }, // Soft
            { 0.8f, 1f, 0.5f, 0.8f }, // Light
            { 0.7f, 1f, 0.3f, 0.5f }, // Hard
            { 0.1f, 0.3f, 0.6f, 0.9f }, // Pale
            { 0.5f, 1f, 0.5f, 0.5f } // Default
        };

        private Color _baseColor;

        public enum Preset {
            Default,
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

        public static Color ConvertColor(Color color, Preset preset) {
            var random = new Random();
            float hue, saturation, brightness;

            ColorExtensions.RGBtoHSB(color.R, color.G, color.B, out hue, out saturation, out brightness);
            saturation = random.Next((int)(hsbRanges[(int)preset, 0] * 100), (int)(hsbRanges[(int)preset, 1] * 100)) / 100f;
            brightness = random.Next((int)(hsbRanges[(int)preset, 2] * 100), (int)(hsbRanges[(int)preset, 3] * 100f)) / 100f;
            hue = (float)random.NextDouble();
            return ColorFromHSB(hue, saturation, brightness);
        }

        public static Preset GetColorPreset(Color color) {
            var random = new Random();
            float hue, saturation, brightness;

            ColorExtensions.RGBtoHSB(color.R, color.G, color.B, out hue, out saturation, out brightness);
            var minDistance = float.MaxValue;
            var closestPreset = Preset.Default;
            for (var i = 0; i < Enum.GetValues(typeof(Preset)).Length; i++) {
                var presetHue = (float)random.NextDouble();
                var presetSaturation = (hsbRanges[i, 0] + hsbRanges[i, 1]) / 2f;
                var presetBrightness = (hsbRanges[i, 2] + hsbRanges[i, 3]) / 2f;
                var distance = (hue - presetHue) * (hue - presetHue) + (saturation - presetSaturation) * (saturation - presetSaturation) + (brightness - presetBrightness) * (brightness - presetBrightness);
                if (distance < minDistance) {
                    minDistance = distance;
                    closestPreset = (Preset)i;
                }
            }

            return closestPreset;
        }

        public Color[] Generate(Color baseColor = default(Color), Scheme scheme = Scheme.Complementary, Preset preset = Preset.Default) {
            // generate a base color if not specified
            if (baseColor == default(Color)) {
                baseColor = GetRandomColor();
                _baseColor = baseColor;
            } else {
                var detectedPreset = GetColorPreset(baseColor);
                if (detectedPreset != preset) {
                    var adjustedBaseColor = ConvertColor(baseColor, preset);
                    _baseColor = baseColor = adjustedBaseColor;
                }
            }

            //return colors;
            switch (scheme) {
                case Scheme.Analogous:
                    return GenerateAnalogousScheme(baseColor);

                case Scheme.Complementary:
                    return GenerateComplementaryScheme(baseColor);

                case Scheme.Monochromatic:
                    return GenerateMonochromaticScheme(baseColor);

                case Scheme.Tetradic:
                    return GenerateTetradicScheme(baseColor);

                case Scheme.Triadic:
                    return GenerateTriadicScheme(baseColor);

                default:
                    throw new Exception("Invalid scheme!");
            }
        }

        public Color[] GenerateAnalogousScheme(Color baseColor) {
            var colors = new Color[14];

            var analogousColors = GetAnalogousColors(baseColor, 5, 1.5f);

            Array.Copy(analogousColors, 0, colors, 0, analogousColors.Length);
            Array.Copy(GetMonochromaticColors(analogousColors[1], 3, 0.2f), 0, colors, 5, 3);
            Array.Copy(GetMonochromaticColors(analogousColors[2], 3, 0.2f), 0, colors, 8, 3);
            Array.Copy(analogousColors, 1, colors, 11, 3);

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

        private Color GetRandomColor(Preset preset = Preset.Default) {
            var random = new Random();

            float hue;
            float saturation;
            float brightness;

            switch (preset) {
                case Preset.Pastel:
                case Preset.Soft:
                case Preset.Light:
                case Preset.Hard:
                case Preset.Pale:

                    // generate colors based on preset values
                    hue = (float)random.NextDouble() * 360;
                    saturation = random.Next((int)(hsbRanges[(int)preset, 0] * 100), (int)(hsbRanges[(int)preset, 1] * 100)) / 100f;
                    brightness = random.Next((int)(hsbRanges[(int)preset, 2] * 100), (int)(hsbRanges[(int)preset, 3] * 100)) / 100f;

                    break;

                default:
                    // generate random colors
                    hue = (float)random.NextDouble() * 360;
                    saturation = random.Next((int)(hsbRanges[(int)Preset.Default, 0] * 100), (int)(hsbRanges[(int)Preset.Default, 1] * 100)) / 100f;
                    brightness = random.Next((int)(hsbRanges[(int)Preset.Default, 2] * 100), (int)(hsbRanges[(int)Preset.Default, 3] * 100)) / 100f;
                    break;
            }

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