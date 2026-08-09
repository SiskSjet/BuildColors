using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Shows a share code, or takes one that was pasted in.
    /// </summary>
    internal class CodeDialog : DialogBase {
        private const float WIDTH = 560f;

        private readonly TextField _field;

        public CodeDialog(string title, string hint, string actionLabel, string initialText = null, HudParentBase parent = null) : base(parent) {
            var contentWidth = WIDTH - Padding.X - LayoutMetrics.CONTENT_PADDING_X;

            var hintLabel = new Label() {
                Text = hint,
                Format = Style.CaptionText,
                AutoResize = false,
                BuilderMode = TextBuilderModes.Wrapped,
                Width = contentWidth,
                Height = LayoutMetrics.LABEL_HEIGHT * 2f,
            };

            _field = ControlFactory.CreateTextField(contentWidth, initialText);
            _field.BuilderMode = TextBuilderModes.Wrapped;
            _field.Height = LayoutMetrics.CONTROL_HEIGHT * 2f;

            var cancelButton = ControlFactory.CreateButton(ModText.BC_UI_Cancel.GetString(), 140f);
            var actionButton = ControlFactory.CreateButton(actionLabel, 140f, ButtonRole.Primary);

            var height = HEADER_HEIGHT
                + Padding.Y
                + LayoutMetrics.CONTENT_PADDING_Y * 2f
                + hintLabel.Height
                + _field.Height
                + LayoutMetrics.BUTTON_HEIGHT
                + LayoutMetrics.ROW_SPACING * 2f;

            Size = new Vector2(WIDTH, height);
            HeaderText = title;

            var layout = new HudChain(true, body) {
                ParentAlignment = ParentAlignments.Inner,
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                DimAlignment = DimAlignments.UnpaddedSize,
                Padding = new Vector2(LayoutMetrics.CONTENT_PADDING_X, LayoutMetrics.CONTENT_PADDING_Y),
            };

            layout.Add(hintLabel, 0f);
            layout.Add(_field, 0f);
            layout.Add(ControlFactory.CreateButtonRow(contentWidth, cancelButton, actionButton), 0f);

            cancelButton.MouseInput.LeftClicked += OnCancel;
            actionButton.MouseInput.LeftClicked += OnAction;
        }

        public event RichHudFramework.EventHandler Submitted;

        public string Text {
            get { return _field.Text.ToString().Trim(); }
        }

        private void OnCancel(object sender, EventArgs args) {
            HudSoundUtils.PlaySound("HudMouseClick");
            Close();
        }

        private void OnAction(object sender, EventArgs args) {
            HudSoundUtils.PlaySound("HudBleep");
            Submitted?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}
