using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Asks before something is thrown away.
    /// </summary>
    internal class ConfirmDialog : DialogBase {
        private const float WIDTH = 420f;

        public ConfirmDialog(string title, string message, string confirmLabel = null, HudParentBase parent = null) : base(parent) {
            var contentWidth = ContentWidth(WIDTH);

            var messageLabel = new Label() {
                Text = message,
                Format = Style.BodyText,
                AutoResize = false,
                VertCenterText = true,
                BuilderMode = TextBuilderModes.Wrapped,
                Width = contentWidth,
                Height = LayoutMetrics.LABEL_HEIGHT * 3f,
            };

            var confirmButton = ControlFactory.CreateButton(confirmLabel ?? ModText.BC_UI_Remove.GetString(), role: confirmLabel == null ? ButtonRole.Danger : ButtonRole.Primary);
            var cancelButton = CreateCancelButton();

            var buttons = ControlFactory.CreateButtonRow(contentWidth, cancelButton, confirmButton);

            var height = HEADER_HEIGHT
                + Padding.Y
                + LayoutMetrics.CONTENT_PADDING_Y * 2f
                + messageLabel.Height
                + LayoutMetrics.BUTTON_HEIGHT
                + LayoutMetrics.SECTION_SPACING;

            Size = new Vector2(WIDTH, height);
            HeaderText = title;

            var layout = CreateContentColumn(LayoutMetrics.SECTION_SPACING);

            layout.Add(messageLabel, 0f);
            layout.Add(buttons, 0f);

            confirmButton.MouseInput.LeftClicked += OnConfirmed;
        }

        public event RichHudFramework.EventHandler Confirmed;

        private void OnConfirmed(object sender, EventArgs args) {
            HudSoundUtils.PlaySound("HudLockingLost");
            Confirmed?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}
