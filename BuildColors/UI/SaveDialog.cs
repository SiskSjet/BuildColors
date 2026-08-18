using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Asks for a name.
    /// </summary>
    internal class SaveDialog : DialogBase {
        private const float WIDTH = 420f;

        private readonly ActionButton _saveButton;
        private readonly TextField _nameField;

        public SaveDialog(string title, string initialName = null, HudParentBase parent = null) : base(parent) {
            var contentWidth = ContentWidth(WIDTH);

            _nameField = ControlFactory.CreateTextField(contentWidth, initialName);

            _saveButton = ControlFactory.CreateButton(ModText.BC_UI_Save.GetString(), role: ButtonRole.Primary);
            var cancelButton = CreateCancelButton();

            var height = HEADER_HEIGHT
                + Padding.Y
                + LayoutMetrics.CONTENT_PADDING_Y * 2f
                + LayoutMetrics.LABEL_HEIGHT
                + LayoutMetrics.CONTROL_HEIGHT
                + LayoutMetrics.BUTTON_HEIGHT
                + LayoutMetrics.ROW_SPACING
                + LayoutMetrics.SECTION_SPACING;

            Size = new Vector2(WIDTH, height);
            HeaderText = title;

            var layout = CreateContentColumn(LayoutMetrics.ROW_SPACING);

            layout.Add(ControlFactory.CreateCaption(ModText.BC_UI_Name.GetString(), contentWidth), 0f);
            layout.Add(_nameField, 0f);
            layout.Add(ControlFactory.CreateButtonRow(contentWidth, cancelButton, _saveButton), 0f);

            _saveButton.MouseInput.LeftClicked += OnSave;
        }

        public event RichHudFramework.EventHandler Saved;

        /// <summary>
        /// The name that was typed, trimmed.
        /// </summary>
        public string Name {
            get { return _nameField.Text.ToString().Trim(); }
        }

        /// <summary>
        /// Enter saves, which is what a dialog holding a single text field should do.
        /// </summary>
        protected override void HandleInput(Vector2 cursorPos) {
            base.HandleInput(cursorPos);

            if (SharedBinds.Enter.IsNewPressed && !string.IsNullOrEmpty(Name)) {
                Save();
            }
        }

        private void OnSave(object sender, EventArgs args) {
            if (string.IsNullOrEmpty(Name)) {
                HudSoundUtils.PlaySound("HudLockingLost");
                return;
            }

            Save();
        }

        private void Save() {
            HudSoundUtils.PlaySound("HudBleep");
            Saved?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}
