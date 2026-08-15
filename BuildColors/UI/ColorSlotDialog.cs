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

            var contentWidth = WIDTH - Padding.X - LayoutMetrics.CONTENT_PADDING_X;

            _picker = new ColorPickerHSV2() {
                Name = ModText.BC_UI_Slot.GetString(slotIndex + 1),
            };

            SetMask(mask);

            var palette = new ColorPaletteSelector() { Width = contentWidth };
            palette.ColorPicked += OnPaletteColorPicked;

            var cancelButton = ControlFactory.CreateButton(ModText.BC_UI_Cancel.GetString(), 140f);
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

            var layout = new HudChain(true, body) {
                ParentAlignment = ParentAlignments.Inner,
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                DimAlignment = DimAlignments.UnpaddedSize,
                Padding = new Vector2(LayoutMetrics.CONTENT_PADDING_X, LayoutMetrics.CONTENT_PADDING_Y),
            };

            layout.Add(_picker, 0f);
            layout.Add(ControlFactory.CreateCaption(ModText.BC_UI_PickFromPalette.GetString(), contentWidth), 0f);
            layout.Add(palette, 0f);
            layout.Add(ControlFactory.CreateButtonRow(contentWidth, cancelButton, saveButton), 0f);

            cancelButton.MouseInput.LeftClicked += OnCancel;
            saveButton.MouseInput.LeftClicked += OnSave;
        }

        public event RichHudFramework.EventHandler Saved;

        public int SlotIndex { get; private set; }

        /// <summary>
        /// The slot as edited.
        /// </summary>
        public ColorMask Mask {
            get {
                var color = _picker.Color;
                var hsv = new Vector3(color.X / 360f, color.Y / 100f, color.Z / 100f);

                return hsv.HSVToColorMask();
            }
        }

        private void SetMask(ColorMask mask) {
            var hsv = ((Vector3)mask).ColorMaskToHSV();

            _picker.Color = new Vector3(hsv.X * 360f, hsv.Y * 100f, hsv.Z * 100f);
        }

        private void OnPaletteColorPicked(ColorMask mask) {
            SetMask(mask);
        }

        private void OnCancel(object sender, EventArgs args) {
            HudSoundUtils.PlaySound("HudMouseClick");
            Close();
        }

        private void OnSave(object sender, EventArgs args) {
            HudSoundUtils.PlaySound("HudBleep");
            Saved?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}
