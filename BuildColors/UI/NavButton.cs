using RichHudFramework.UI;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// One entry of the panel's navigation rail.
    /// </summary>
    internal class NavButton : LabelBoxButton {
        public const float HEIGHT = 46f;

        /// <summary>
        /// Width of the bar marking the selected entry.
        /// </summary>
        private const float MARKER_WIDTH = 3f;

        private readonly TexturedBox _marker;
        private readonly TexturedBox _selection;

        public NavButton(string text, float width, HudParentBase parent = null) : base(parent) {
            AutoResize = false;
            Size = new Vector2(width, HEIGHT);
            TextPadding = new Vector2(24f, 0f);
            Padding = Vector2.Zero;

            Color = VRageMath.Color.Transparent;
            HighlightColor = Style.HoverBackgroundColor;
            HighlightEnabled = true;

            _selection = new TexturedBox(this) {
                DimAlignment = DimAlignments.Both,
                Color = Style.SelectionBackgroundColor,
                Visible = false,
                ZOffset = -1,
            };

            _marker = new TexturedBox(this) {
                DimAlignment = DimAlignments.Height,
                Width = MARKER_WIDTH,
                ParentAlignment = ParentAlignments.Left | ParentAlignments.Inner,
                Color = Style.AccentColor,
                Visible = false,
            };

            Text = text;
            Format = Style.BodyText;

            MouseInput.CursorEntered += (sender, args) => HudSoundUtils.PlaySound("HudMouseOver");
        }

        public bool Selected {
            set {
                _selection.Visible = value;
                _marker.Visible = value;

                Format = value ? Style.BodyText.WithColor(Style.AccentColor) : Style.BodyText;
            }
        }
    }
}
