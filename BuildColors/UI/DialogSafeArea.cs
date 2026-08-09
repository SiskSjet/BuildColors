using RichHudFramework.UI.Client;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Resolves where a dialog may be placed.
    /// </summary>
    internal static class DialogSafeArea {
        /// <summary>
        /// Gap kept between a dialog and the edge of the region it is placed in.
        /// </summary>
        public const float MARGIN = 20f;

        /// <summary>
        /// Screen dimensions in the coordinate space used by elements parented to the high DPI root.
        /// </summary>
        public static Vector2 ScreenSize {
            get {
                var size = HudMain.ScreenDimHighDPI;
                return size.X > 0f && size.Y > 0f ? size : new Vector2(1920f, 1080f);
            }
        }

        /// <summary>
        /// Right hand boundary of the free region.
        /// </summary>
        public static float GetRightBound() {
            return PickerScreenLayout.RightBound;
        }

        /// <summary>
        /// Width of the free region.
        /// </summary>
        public static float GetAvailableWidth() {
            return GetRightBound() - (-ScreenSize.X * .5f + MARGIN);
        }

        /// <summary>
        /// Returns the offset that centers a dialog of the given size inside the free region.
        /// </summary>
        public static Vector2 GetCenterOffset(Vector2 dialogSize) {
            var screenSize = ScreenSize;

            var leftBound = -screenSize.X * .5f + MARGIN;
            var rightBound = GetRightBound();

            var halfWidth = dialogSize.X * .5f;
            var offsetX = (leftBound + rightBound) * .5f;

            if (offsetX + halfWidth > rightBound) {
                offsetX = rightBound - halfWidth;
            }

            if (offsetX - halfWidth < leftBound) {
                offsetX = leftBound + halfWidth;
            }

            var halfHeight = dialogSize.Y * .5f;
            var topBound = screenSize.Y * .5f - MARGIN;
            var bottomBound = -screenSize.Y * .5f + MARGIN;
            var offsetY = 0f;

            if (offsetY + halfHeight > topBound) {
                offsetY = topBound - halfHeight;
            }

            if (offsetY - halfHeight < bottomBound) {
                offsetY = bottomBound + halfHeight;
            }

            return new Vector2(offsetX, offsetY);
        }
    }
}
