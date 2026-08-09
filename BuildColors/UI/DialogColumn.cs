using RichHudFramework.UI;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Builds the scrolling columns the dialogs stack their controls in.
    /// <para>
    /// A plain <see cref="HudChain" /> does not clip: when its members add up to more than it has room for,
    /// it squeezes their positions into the space it has and they end up drawn on top of one another rather
    /// than being cut off. That makes every column a standing risk, because one control measuring taller
    /// than expected silently corrupts the whole column. A <see cref="ScrollBox" /> clips instead and offers
    /// a scrollbar, so the worst an over-full column can do is ask the reader to scroll.
    /// </para>
    /// </summary>
    internal static class DialogColumn {

        /// <summary>
        /// Width the vertical scrollbar occupies, taken out of the column as padding so that members are
        /// laid out beside the bar instead of underneath it.
        /// </summary>
        public const float SCROLLBAR_WIDTH = 43f;

        /// <summary>
        /// Width left for controls in a column of the given total width.
        /// </summary>
        public static float ContentWidth(float columnWidth) {
            return columnWidth - SCROLLBAR_WIDTH;
        }

        public static ScrollBox Create(float width, float height) {
            // Padding is set before the size: the width setter takes the padding off what it is given, so a
            // padding applied afterwards would widen the column by the width of the scrollbar instead.
            return new ScrollBox(true) {
                Padding = new Vector2(SCROLLBAR_WIDTH, 0f),
                Width = width,
                Height = height,
                Spacing = LayoutMetrics.SECTION_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                EnableScrolling = true,
            };
        }
    }
}
