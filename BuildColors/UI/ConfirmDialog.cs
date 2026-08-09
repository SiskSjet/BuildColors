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
            var contentWidth = WIDTH - Padding.X - LayoutMetrics.CONTENT_PADDING_X;

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
            var cancelButton = ControlFactory.CreateButton(ModText.BC_UI_Cancel.GetString());

            var buttons = ControlFactory.CreateButtonRow(contentWidth, cancelButton, confirmButton);

            var height = HEADER_HEIGHT
                + Padding.Y
                + LayoutMetrics.CONTENT_PADDING_Y * 2f
                + messageLabel.Height
                + LayoutMetrics.BUTTON_HEIGHT
                + LayoutMetrics.SECTION_SPACING;

            Size = new Vector2(WIDTH, height);
            HeaderText = title;

            var layout = new HudChain(true, body) {
                ParentAlignment = ParentAlignments.Inner,
                Spacing = LayoutMetrics.SECTION_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                DimAlignment = DimAlignments.UnpaddedSize,
                Padding = new Vector2(LayoutMetrics.CONTENT_PADDING_X, LayoutMetrics.CONTENT_PADDING_Y),
            };

            layout.Add(messageLabel, 0f);
            layout.Add(buttons, 0f);

            confirmButton.MouseInput.LeftClicked += OnConfirmed;
            cancelButton.MouseInput.LeftClicked += OnCancelled;
        }

        public event RichHudFramework.EventHandler Confirmed;

        private void OnCancelled(object sender, EventArgs args) {
            HudSoundUtils.PlaySound("HudMouseClick");
            Close();
        }

        private void OnConfirmed(object sender, EventArgs args) {
            HudSoundUtils.PlaySound("HudLockingLost");
            Confirmed?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}
