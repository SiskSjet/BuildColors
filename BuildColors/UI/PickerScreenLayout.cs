using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Works out the region of the color picker screen this mod may draw in, in <see cref="UiSpace"/> units.
    /// </summary>
    internal static class PickerScreenLayout {
        public const float ASPECT_RATIO_END = 5f / 4f;
        public const float ASPECT_RATIO_START = 16f / 9f;

        /// <summary>
        /// Left edge of the vanilla controls at each measured aspect ratio, a gap short of their background.
        /// </summary>
        public const float BOUNDARY_AT_ASPECT_END = 190f;

        public const float BOUNDARY_AT_ASPECT_START = 450f;

        public const float SLOPE = (BOUNDARY_AT_ASPECT_END - BOUNDARY_AT_ASPECT_START) / (ASPECT_RATIO_END - ASPECT_RATIO_START);
        public const float Y_INTERCEPT = BOUNDARY_AT_ASPECT_START - SLOPE * ASPECT_RATIO_START;

        /// <summary>
        /// Width the vanilla controls take at 16:9, which is what they take everywhere if their panel is one
        /// fixed width pinned to the right edge of the screen.
        /// </summary>
        public const float VANILLA_WIDTH = UiSpace.REFERENCE_HEIGHT * ASPECT_RATIO_START * .5f - BOUNDARY_AT_ASPECT_START;

        /// <summary>
        /// Right hand boundary of the region the mod may use. The measurements disagree on how the vanilla
        /// panel behaves off 16:9, so the narrower of what each implies wins.
        /// </summary>
        public static float RightBound {
            get {
                var screen = DialogSafeArea.ScreenSize;
                var aspectRatio = Math.Max(screen.X / screen.Y, ASPECT_RATIO_END);

                var measured = SLOPE * aspectRatio + Y_INTERCEPT;
                var fixedWidth = screen.X * .5f - VANILLA_WIDTH * (screen.Y / UiSpace.REFERENCE_HEIGHT);

                return Math.Min(measured, fixedWidth);
            }
        }

        public static float LeftBound {
            get { return -DialogSafeArea.ScreenSize.X * .5f + LayoutMetrics.SCREEN_GAP; }
        }

        /// <summary>
        /// Size of the largest panel that fits the region.
        /// </summary>
        public static Vector2 PanelSize {
            get {
                var screen = DialogSafeArea.ScreenSize;

                return new Vector2(RightBound - LeftBound, screen.Y - LayoutMetrics.SCREEN_GAP * 2f);
            }
        }

        /// <summary>
        /// Offset that places a panel of PanelSize in the region.
        /// </summary>
        public static Vector2 PanelOffset {
            get { return new Vector2((LeftBound + RightBound) * .5f, 0f); }
        }
    }
}
