using RichHudFramework.UI.Client;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Resolves where a dialog may be placed. The vanilla color picker controls always draw on top of the
    /// framework HUD, so dialogs are kept inside the free region rather than centered on the screen.
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
        /// Right hand boundary of the free region, which is the right edge of the color management panel.
        /// Dialogs may cover the mod panels, but everything further right belongs to the vanilla controls,
        /// which always draw on top.
        /// </summary>
        public static float GetRightBound() {
            var screenSize = ScreenSize;
            var aspectRatio = screenSize.X / screenSize.Y;
            var panelAnchor = ColorManagementPanel.SLOPE * aspectRatio + ColorManagementPanel.Y_INTERCEPT;

            return panelAnchor + ColorManagementPanel.WIDTH * .5f;
        }

        /// <summary>
        /// Width of the free region. Dialogs should not exceed it.
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

            // Keep the dialog inside the region; if it cannot fit, pin it to the left edge so the
            // controls on its left stay reachable.
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
