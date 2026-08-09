using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System.Globalization;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Puts a paint source into words.
    /// </summary>
    internal static class PaintSourceText {
        public static string Describe(PaintColorSource source) {
            if (source == null) {
                return DescribeType(PaintSourceType.Solid);
            }

            switch (source.Type) {
                case PaintSourceType.Gradient:
                    return ModText.BC_Desc_SourceGradient.GetString(
                        CountStops(source),
                        DescribeAxis(source.Axis),
                        source.Steps > 0
                            ? ModText.BC_Desc_SourceBands.GetString(source.Steps)
                            : ModText.BC_Desc_SourceSmooth.GetString(),
                        source.Fit == PaintGradientFit.GridBounds
                            ? ModText.BC_Desc_SourceFitGrid.GetString()
                            : ModText.BC_Desc_SourceFitBlocks.GetString());

                case PaintSourceType.Camo:
                    return ModText.BC_Desc_SourceCamo.GetString(CountPalette(source), source.Scale.ToString("0.##", CultureInfo.CurrentCulture));

                case PaintSourceType.Scatter:
                    return ModText.BC_Desc_SourceScatter.GetString(CountPalette(source));

                case PaintSourceType.Pattern:
                    return ModText.BC_Desc_SourcePattern.GetString(
                        DescribeShape(source.Shape),
                        CountPalette(source),
                        source.Period,
                        DescribeAxis(source.Axis));

                default:
                    return DescribeType(PaintSourceType.Solid);
            }
        }

        public static string DescribeType(PaintSourceType type) {
            switch (type) {
                case PaintSourceType.Gradient:
                    return ModText.BC_UI_SourceType_Gradient.GetString();

                case PaintSourceType.Camo:
                    return ModText.BC_UI_SourceType_Camo.GetString();

                case PaintSourceType.Scatter:
                    return ModText.BC_UI_SourceType_Scatter.GetString();

                case PaintSourceType.Pattern:
                    return ModText.BC_UI_SourceType_Pattern.GetString();

                default:
                    return ModText.BC_UI_SourceType_Solid.GetString();
            }
        }

        public static string DescribeHint(PaintSourceType type) {
            switch (type) {
                case PaintSourceType.Gradient:
                    return ModText.BC_UI_SourceHint_Gradient.GetString();

                case PaintSourceType.Camo:
                    return ModText.BC_UI_SourceHint_Camo.GetString();

                case PaintSourceType.Scatter:
                    return ModText.BC_UI_SourceHint_Scatter.GetString();

                case PaintSourceType.Pattern:
                    return ModText.BC_UI_SourceHint_Pattern.GetString();

                default:
                    return ModText.BC_UI_SourceHint_Solid.GetString();
            }
        }

        public static string DescribeAxis(PaintSourceAxis axis) {
            switch (axis) {
                case PaintSourceAxis.GridX:
                    return ModText.BC_UI_Axis_GridX.GetString();

                case PaintSourceAxis.GridZ:
                    return ModText.BC_UI_Axis_GridZ.GetString();

                case PaintSourceAxis.WorldUp:
                    return ModText.BC_UI_Axis_WorldUp.GetString();

                case PaintSourceAxis.Radial:
                    return ModText.BC_UI_Axis_Radial.GetString();

                case PaintSourceAxis.Longest:
                    return ModText.BC_UI_Axis_Longest.GetString();

                default:
                    return ModText.BC_UI_Axis_GridY.GetString();
            }
        }

        public static string DescribeShape(PaintPatternShape shape) {
            return shape == PaintPatternShape.Checker
                ? ModText.BC_UI_Shape_Checker.GetString()
                : ModText.BC_UI_Shape_Stripes.GetString();
        }

        private static int CountStops(PaintColorSource source) {
            return source.Stops != null ? source.Stops.Count : 0;
        }

        private static int CountPalette(PaintColorSource source) {
            return source.Palette != null ? source.Palette.Count : 0;
        }
    }
}
