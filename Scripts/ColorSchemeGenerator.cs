using Sisk.BuildColors.Settings.Models.ColorSpace;
using System;
using System.Linq;
using VRage.Utils;
using VRageMath;

namespace Sisk.BuildColors {

    public class ColorSchemeGenerator {

        private static readonly float[,] _presetRanges = {
            { 0.0f, 1.0f, 0.0f, 1.0f, 0f, 360f }, // Default
            { 0.2f, 0.4f, 0.8f, 0.9f, 0f, 360f }, // Pastel
            { 0.3f, 0.6f, 0.6f, 0.8f, 0f, 360f }, // Soft
            { 0.8f, 1.0f, 0.9f, 1.0f, 0f, 360f }, // Light
            { 0.5f, 1.0f, 0.0f, 0.5f, 0f, 360f }, // Hard
            { 0.5f, 0.7f, 0.9f, 1.0f, 0f, 360f }, // Pale
            { 0.5f, 1.0f, 0.5f, 1.0f, 0f, 360f }, // Vibrant
            { 0.0f, 0.3f, 0.3f, 0.6f, 0f, 60f }, // Muted
            { 0.5f, 0.8f, 0.4f, 0.6f, 30f, 60f }, // Warm
            { 0.2f, 0.5f, 0.4f, 0.7f, 180f, 270f }, // Cool
            { 0.2f, 0.5f, 0.5f, 1.0f, 0f, 360f }, // Dark
            { 0.5f, 1.0f, 0.8f, 1.0f, 0f, 360f } // Lighter
        };

        private readonly Random _random = new Random();
        private HSL _baseColor;

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
            Monochromatic,
            Tetradic,
            Triadic
        }

        public HSL BaseColor {
            get { return _baseColor; }
            set { _baseColor = value; }
        }

        public static HSL[] GenerateAnalogousScheme(HSL color) {
            return GetAnalogousColors(color, 14);
        }

        public static HSL[] GenerateComplementaryScheme(HSL color) {
            var colors = new HSL[14];

            var baseMono = GetMonochromaticColors(color, 7, .5f);
            var complementary = GetComplementaryColor(color);
            var compMono = GetNeutralColors(complementary, 7);

            var index = 0;
            Array.Copy(baseMono, colors, baseMono.Length);
            index += baseMono.Length;
            Array.Copy(compMono, 0, colors, index, compMono.Length);

            return colors;
        }

        public static HSL[] GenerateDefaultColorScheme(HSL baseColor) {
            var colors = new HSL[14];

            var monocromatic = GetMonochromaticColors(baseColor, 4, .5f);
            var neutralColors = GetNeutralColors(baseColor, 4);

            // Get the complementary color
            var complementary = GetComplementaryColor(baseColor);
            // Generate 2 monochromatic colors from complementary
            var complementaryColors = GetMonochromaticColors(complementary, 2, .3f);

            // Get split complementary colors from base color
            var split = GetSplitComplementaryColors(baseColor).Skip(1).Take(2).ToArray();
            var split2 = GetSplitComplementaryColors(complementary).Skip(1).Take(2).ToArray();

            var index = 0;
            Array.Copy(monocromatic, colors, monocromatic.Length);

            index += monocromatic.Length;
            Array.Copy(neutralColors, 0, colors, index, neutralColors.Length);

            index += neutralColors.Length;
            Array.Copy(complementaryColors, 0, colors, index, complementaryColors.Length);

            index += complementaryColors.Length;
            Array.Copy(split, 0, colors, index, split.Length);

            index += split.Length;
            Array.Copy(split2, 0, colors, index, split2.Length);

            return colors;
        }

        public static HSL[] GenerateMonochromaticScheme(HSL color) {
            return GetMonochromaticColors(color, 14);
        }

        public static HSL[] GenerateTetradicScheme(HSL color) {
            var colors = new HSL[14];

            var tetradic = GetTetradicColors(color);
            var monoBase = GetMonochromaticColors(tetradic[0], 4, .4f);
            var monoComp = GetMonochromaticColors(tetradic[2], 4, .4f);
            var mono3 = GetMonochromaticColors(tetradic[1], 3, .5f);
            var mono4 = GetMonochromaticColors(tetradic[3], 3, .5f);

            var index = 0;
            Array.Copy(monoBase, colors, monoBase.Length);
            index += monoBase.Length;

            Array.Copy(monoComp, 0, colors, index, monoComp.Length);
            index += monoComp.Length;

            Array.Copy(mono3, 0, colors, index, mono3.Length);
            index += mono3.Length;

            Array.Copy(mono4, 0, colors, index, mono4.Length);

            return colors;
        }

        public static HSL[] GenerateTriadicScheme(HSL color) {
            var colors = new HSL[14];
            var triadic = GetTriadicColors(color);
            var monoBase = GetMonochromaticColors(triadic[0], 7, .6f);
            var mono2 = GetMonochromaticColors(triadic[1], 4, .5f);
            var mono3 = GetMonochromaticColors(triadic[2], 3, .5f);

            var index = 0;
            Array.Copy(monoBase, colors, monoBase.Length);
            index += monoBase.Length;

            Array.Copy(mono2, 0, colors, index, mono2.Length);
            index += mono2.Length;

            Array.Copy(mono3, 0, colors, index, mono3.Length);

            return colors;
        }

        public static HSL[] GetAnalogousColors(HSL color, int numColors, float angle = 30f, float range = 0.1f) {
            // Get the base color values in HSL color space
            var hue = color.H;
            var saturation = color.S;
            var lightness = color.L;

            // Calculate the hue values for the other colors in the analogous scheme
            var hues = new float[numColors];
            hues[0] = hue;

            for (var i = 1; i < numColors; i++) {
                var angleIncrement = i * angle;
                var hue1 = (hue + angleIncrement) % 360f;
                var hue2 = (hue - angleIncrement + 360f) % 360f;
                hues[i] = Math.Abs(hue1 - hue) < Math.Abs(hue2 - hue) ? hue1 : hue2;
            }

            // Create a new array to hold the generated colors
            var colors = new HSL[numColors];

            // Create the analogous colors
            for (var i = 0; i < numColors; i++) {
                var hueValue = hues[i];
                var saturationValue = Math.Max(0, Math.Min(1, saturation + range * (float)Math.Cos(angle * i * Math.PI / 180)));
                var lightnessValue = Math.Max(0, Math.Min(1, lightness + range * (float)Math.Sin(angle * i * Math.PI / 180)));
                colors[i] = new HSL(hueValue, saturationValue, lightnessValue);
            }

            return colors;
        }

        public static HSL GetComplementaryColor(HSL color) {
            // Get the base color values in HSL color space
            var hue = color.H;
            var saturation = color.S;
            var lightness = color.L;

            // Calculate the hue value for the complementary color
            var hueComplementary = (hue + 180f) % 360f;

            // Create the complementary color
            return new HSL(hueComplementary, saturation, lightness);
        }

        public static HSL[] GetMonochromaticColors(HSL color, int numShades, float lightnessRange = 0.8f) {
            // Get the base color values in HSL color space
            var hue = color.H;
            var saturation = color.S;
            var lightness = color.L;

            // Create a new array to hold the generated colors
            var colors = new HSL[numShades];

            // Determine the valid range of lightness values
            var minLightness = Math.Max(0.1f, lightness - (lightnessRange / 2));
            var maxLightness = Math.Min(1, lightness + (lightnessRange / 2));
            var lightnessStep = (maxLightness - minLightness) / (numShades - 1);

            // Generate the shades
            for (var i = 0; i < numShades; i++) {
                var l = minLightness + i * lightnessStep;
                colors[i] = new HSL(hue, saturation, l);
            }

            return colors;
        }

        public static HSL[] GetNeutralColors(HSL baseColor, int numColors) {
            var colors = new HSL[numColors];

            // Determine the saturation and lightness steps
            var saturationStep = baseColor.S / (numColors + 1);
            var lightnessStep = (1 - baseColor.L) / (numColors + 1);

            for (var i = 0; i < numColors; i++) {
                // Calculate the saturation and lightness for the current neutral color
                var saturation = baseColor.S - (i + 1) * saturationStep;
                var lightness = baseColor.L + (i + 1) * lightnessStep;

                // Create a neutral color with the calculated saturation and lightness
                colors[i] = new HSL(baseColor.H, saturation, lightness);
            }

            return colors;
        }

        public static HSL[] GetSplitComplementaryColors(HSL color, float angle = 150f, float range = 0.1f) {
            // Get the base color values in HSL color space
            var hue = color.H;
            var saturation = color.S;
            var lightness = color.L;

            // Calculate the hue values for the other colors in the split complementary scheme
            var hue1 = (hue + angle) % 360f;
            var hue2 = (hue - angle + 360f) % 360f;

            // Create a new array to hold the generated colors
            var colors = new HSL[3];

            // Create the split complementary colors
            colors[0] = new HSL(hue, saturation, lightness);
            colors[1] = new HSL(hue1, saturation, Math.Max(0, Math.Min(1, lightness + range)));
            colors[2] = new HSL(hue2, saturation, Math.Max(0, Math.Min(1, lightness + range)));

            return colors;
        }

        public static HSL[] GetTetradicColors(HSL color, float angle = 60f) {
            // Get the base color values in HSL color space
            var hue = color.H;
            var saturation = color.S;
            var lightness = color.L;

            // Calculate the hue values for the other colors in the tetradic scheme
            var hue1 = hue + angle;
            var hue2 = hue + 180f;
            var hue3 = hue1 + 180f;

            // Normalize the hue values to be between 0 and 360
            hue1 %= 360f;
            hue2 %= 360f;
            hue3 %= 360f;

            // Create a new array to hold the generated colors
            var colors = new HSL[4];

            // Create the tetradic colors
            colors[0] = new HSL(hue, saturation, lightness);
            colors[1] = new HSL(hue1, saturation, lightness);
            colors[2] = new HSL(hue2, saturation, lightness);
            colors[3] = new HSL(hue3, saturation, lightness);

            return colors;
        }

        public static HSL[] GetTriadicColors(HSL color, float angle = 60f) {
            // Get the base color values in HSL color space
            var hue = color.H;
            var saturation = color.S;
            var lightness = color.L;

            // Calculate the hue values for the other colors in the triadic scheme

            var complementary = (hue + 180f) % 360f;
            var hue1 = complementary + angle;
            var hue2 = complementary - angle;

            // Normalize the hue values to be between 0 and 360
            hue1 %= 360f;
            hue2 %= 360f;

            // Create a new array to hold the generated colors
            var colors = new HSL[3];

            // Create the triadic colors
            colors[0] = new HSL(hue, saturation, lightness);
            colors[1] = new HSL(hue1, saturation, lightness);
            colors[2] = new HSL(hue2, saturation, lightness);

            return colors;
        }

        public HSL ConvertColor(HSL color, Preset preset) {
            if (!Enum.IsDefined(typeof(Preset), preset)) {
                preset = Preset.None;
            }

            var minSaturation = _presetRanges[(int)preset, 0];
            var maxSaturation = _presetRanges[(int)preset, 1];
            var minLightness = _presetRanges[(int)preset, 2];
            var maxLightness = _presetRanges[(int)preset, 3];
            var minHue = _presetRanges[(int)preset, 4];
            var maxHue = _presetRanges[(int)preset, 5];

            var baseHue = color.H;
            var baseSaturation = color.S;
            var baseLightness = color.L;

            var hue = (float)(minHue + (maxHue - minHue) * (baseHue / 360));
            var saturation = (float)(minSaturation + (maxSaturation - minSaturation) * baseSaturation);
            var lightness = (float)(minLightness + (maxLightness - minLightness) * baseLightness);

            return new HSL(hue, saturation, lightness);
        }

        public HSL[] Generate(HSL? color = null, Scheme scheme = Scheme.Complementary, Preset preset = Preset.None) {
            // generate a base color if not specified
            var baseColor = !color.HasValue ? GetRandomColor() : color.Value;

            _baseColor = baseColor;
            var adjustedColor = ConvertColor(baseColor, preset);

            if (color.HasValue && preset == Preset.None) {
                adjustedColor = baseColor;
            }

            HSL[] colors;
            //return colors;
            switch (scheme) {
                case Scheme.Default:
                    colors = GenerateDefaultColorScheme(baseColor);
                    break;

                case Scheme.Analogous:
                    colors = GenerateAnalogousScheme(baseColor);
                    break;

                case Scheme.Complementary:
                    colors = GenerateComplementaryScheme(baseColor);
                    break;

                case Scheme.Monochromatic:
                    colors = GenerateMonochromaticScheme(baseColor);
                    break;

                case Scheme.Tetradic:
                    colors = GenerateTetradicScheme(baseColor);
                    break;

                case Scheme.Triadic:
                    colors = GenerateTriadicScheme(baseColor);
                    break;

                default:
                    throw new Exception("Invalid scheme!");
            }

            MyLog.Default.Warning("Before Generated colorset with sheme {0} preset {1} and adjustedColor {2}", scheme, preset, adjustedColor);
            for (var i = 0; i < colors.Length; i++) {
                MyLog.Default.Warning(colors[i].ToString());
                colors[i] = ConvertColor(colors[i], preset);
            }

            MyLog.Default.Warning("Generated colorset with sheme {0} preset {1} and adjustedColor {2}", scheme, preset, adjustedColor);
            foreach (var item in colors) {
                MyLog.Default.Warning(item.ToString());
            }
            return colors;
        }

        public HSL GetRandomColor() {
            var random = _random;

            // Generate random hue, saturation, and lightness values
            var hue = random.Next(0, 360);
            var saturation = (float)random.NextDouble();
            var lightness = (float)random.NextDouble();

            // Create and return the color
            return new HSL(hue, saturation, lightness);
        }
    }
}