using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Works out the region of the color picker screen this mod may draw in.
    /// </summary>
    internal static class PickerScreenLayout {
        public const float ASPECT_RATIO_END = 5f / 4f;
        public const float ASPECT_RATIO_START = 16f / 9f;

        /// <summary>
        /// Left edge of the vanilla controls at each measured aspect ratio.
        /// </summary>
        public const float BOUNDARY_AT_ASPECT_END = 190f;

        public const float BOUNDARY_AT_ASPECT_START = 450f;

        public const float SLOPE = (BOUNDARY_AT_ASPECT_END - BOUNDARY_AT_ASPECT_START) / (ASPECT_RATIO_END - ASPECT_RATIO_START);
        public const float Y_INTERCEPT = BOUNDARY_AT_ASPECT_START - SLOPE * ASPECT_RATIO_START;

        /// <summary>
        /// Gap left between the mod panel and the vanilla controls.
        /// </summary>
        public const float VANILLA_GAP = 16f;

        /// <summary>
        /// Gap left at the left edge of the screen.
        /// </summary>
        public const float SIDE_MARGIN = 24f;

        public const float TOP_MARGIN = 40f;

        /// <summary>
        /// Space kept clear at the bottom of the screen for the game's toolbar and status readouts.
        /// </summary>
        public const float BOTTOM_RESERVE = 150f;

        /// <summary>
        /// Right hand boundary of the region the mod may use.
        /// </summary>
        public static float RightBound {
            get {
                var screen = DialogSafeArea.ScreenSize;
                var aspectRatio = screen.X / screen.Y;

                return SLOPE * aspectRatio + Y_INTERCEPT - VANILLA_GAP;
            }
        }

        public static float LeftBound {
            get { return -DialogSafeArea.ScreenSize.X * .5f + SIDE_MARGIN; }
        }

        /// <summary>
        /// Size of the largest panel that fits the region.
        /// </summary>
        public static Vector2 PanelSize {
            get {
                var screen = DialogSafeArea.ScreenSize;

                return new Vector2(RightBound - LeftBound, screen.Y - TOP_MARGIN - BOTTOM_RESERVE);
            }
        }

        /// <summary>
        /// Offset that places a panel of PanelSize in the region.
        /// </summary>
        public static Vector2 PanelOffset {
            get {
                var screen = DialogSafeArea.ScreenSize;

                var top = screen.Y * .5f - TOP_MARGIN;
                var bottom = -screen.Y * .5f + BOTTOM_RESERVE;

                return new Vector2((LeftBound + RightBound) * .5f, (top + bottom) * .5f);
            }
        }
    }
}
