using RichHudFramework.UI;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// What a button is for, which is all the palette needs to know to color it.
    /// </summary>
    internal enum ButtonRole {
        /// <summary>
        /// Everything that is neither the obvious next step nor destructive.
        /// </summary>
        Default,

        /// <summary>
        /// The one action a screen exists for.
        /// </summary>
        Primary,

        /// <summary>
        /// Deletes something.
        /// </summary>
        Danger,
    }

    /// <summary>
    /// A bordered button that looks disabled when it is.
    /// </summary>
    internal class ActionButton : BorderedButton {
        private readonly ButtonRole _role;
        private bool _wasEnabled = true;

        public ActionButton(ButtonRole role = ButtonRole.Default, HudParentBase parent = null) : base(parent) {
            _role = role;

            Padding = Vector2.Zero;
            TextPadding = new Vector2(14f, 0f);
            Height = LayoutMetrics.BUTTON_HEIGHT;
            BorderThickness = 1f;
            UseFocusFormatting = false;

            ApplyEnabledColors();

            MouseInput.CursorEntered += (sender, args) => HudSoundUtils.PlaySound("HudMouseOver");
        }

        protected override void Layout() {
            base.Layout();

            if (InputEnabled != _wasEnabled) {
                _wasEnabled = InputEnabled;

                if (InputEnabled) {
                    ApplyEnabledColors();
                } else {
                    ApplyDisabledColors();
                }
            }
        }

        private void ApplyEnabledColors() {
            HighlightEnabled = true;

            switch (_role) {
                case ButtonRole.Primary:
                    Color = Style.AccentBackgroundColor;
                    HighlightColor = Style.AccentHighlightColor;
                    BorderColor = Style.AccentColor;
                    Format = Style.ButtonText.WithColor(Style.BodyTextColor);
                    break;

                case ButtonRole.Danger:
                    Color = Style.ButtonBackgroundColor;
                    HighlightColor = Style.DangerHighlightColor;
                    BorderColor = Style.DangerColor;
                    Format = Style.ButtonText.WithColor(Style.DangerColor);
                    break;

                default:
                    Color = Style.ButtonBackgroundColor;
                    HighlightColor = Style.ButtonHighlightBackgroundColor;
                    BorderColor = Style.ButtonBorderColor;
                    Format = Style.ButtonText;
                    break;
            }
        }

        /// <summary>
        /// Applies the disabled palette and switches highlighting off with it.
        /// </summary>
        private void ApplyDisabledColors() {
            HighlightEnabled = false;

            Color = Style.SunkenBackgroundColor;
            HighlightColor = Style.SunkenBackgroundColor;
            BorderColor = Style.SubtleBorderColor;
            Format = Style.ButtonText.WithColor(Style.DisabledTextColor);
        }
    }
}
