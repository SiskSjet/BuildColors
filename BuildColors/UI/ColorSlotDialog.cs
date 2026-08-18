using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models;
using Sisk.Utils.Localization.Extensions;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Editor for a single build color slot.
    /// </summary>
    internal class ColorSlotDialog : DialogBase {
        private const float WIDTH = 420f;

        private readonly ColorPickerHSV2 _picker;

        public ColorSlotDialog(int slotIndex, ColorMask mask, HudParentBase parent = null) : base(parent) {
            SlotIndex = slotIndex;

            var contentWidth = ContentWidth(WIDTH);

            _picker = new ColorPickerHSV2() {
                Name = ModText.BC_UI_Slot.GetString(slotIndex + 1),
            };

            Mask = mask;

            var palette = new ColorPaletteSelector() { Width = contentWidth };
            palette.ColorPicked += OnPaletteColorPicked;

            var cancelButton = CreateCancelButton(140f);
            var saveButton = ControlFactory.CreateButton(ModText.BC_UI_Save.GetString(), 140f, ButtonRole.Primary);

            var height = HEADER_HEIGHT
                + Padding.Y
                + LayoutMetrics.CONTENT_PADDING_Y * 2f
                + _picker.Height
                + LayoutMetrics.LABEL_HEIGHT
                + ColorPaletteSelector.TOTAL_HEIGHT
                + LayoutMetrics.BUTTON_HEIGHT
                + LayoutMetrics.ROW_SPACING * 3f;

            Size = new Vector2(WIDTH, height);
            HeaderText = ModText.BC_UI_EditSlot.GetString(slotIndex + 1);

            var layout = CreateContentColumn(LayoutMetrics.ROW_SPACING);

            layout.Add(_picker, 0f);
            layout.Add(ControlFactory.CreateCaption(ModText.BC_UI_PickFromPalette.GetString(), contentWidth), 0f);
            layout.Add(palette, 0f);
            layout.Add(ControlFactory.CreateButtonRow(contentWidth, cancelButton, saveButton), 0f);

            saveButton.MouseInput.LeftClicked += OnSave;
        }

        public event RichHudFramework.EventHandler Saved;

        public int SlotIndex { get; private set; }

        /// <summary>
        /// The slot as edited.
        /// </summary>
        public ColorMask Mask {
            get { return _picker.Color; }
            set { _picker.Color = value; }
        }

        private void OnPaletteColorPicked(ColorMask mask) {
            Mask = mask;
        }

        private void OnSave(object sender, EventArgs args) {
            HudSoundUtils.PlaySound("HudBleep");
            Saved?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}
