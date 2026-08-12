using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// The space the interface is laid out in. Rich Hud only normalizes resolutions above 1080p, so below
    /// that a layout of fixed sizes takes a larger share of the screen than it was drawn for. Scaling the
    /// tree keeps the space a constant height everywhere, leaving only the aspect ratio to lay out for.
    /// </summary>
    internal static class UiSpace {
        public const float REFERENCE_HEIGHT = 1080f;

        /// <summary>
        /// Floor on the scale, so text stays readable on a very small screen. Below it the space grows
        /// taller than <see cref="REFERENCE_HEIGHT"/> instead.
        /// </summary>
        public const float MIN_SCALE = .6f;

        private static readonly Vector2 FallbackScreenSize = new Vector2(1920f, 1080f);

        public static float Scale {
            get { return MathHelper.Clamp(ScreenSize.Y / REFERENCE_HEIGHT, MIN_SCALE, 1f); }
        }

        public static Vector2 Size {
            get { return ScreenSize / Scale; }
        }

        private static Vector2 ScreenSize {
            get {
                var size = HudMain.ScreenDimHighDPI;

                return size.X > 0f && size.Y > 0f ? size : FallbackScreenSize;
            }
        }
    }

    /// <summary>
    /// Root node that draws its children in <see cref="UiSpace"/>.
    /// </summary>
    internal class UiSpaceRoot : ScaledSpaceNode {
        public UiSpaceRoot() : base(HudMain.HighDpiRoot) {
            UpdateScaleFunc = () => UiSpace.Scale;
        }

        protected override void Layout() {
            base.Layout();

            HudElementBase.ElementUtils.UpdateRootAnchoring(UiSpace.Size, children);
        }
    }
}
