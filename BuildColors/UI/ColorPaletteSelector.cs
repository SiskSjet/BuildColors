using RichHudFramework.UI;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Row of swatches showing the player's current build color palette. Clicking a swatch reports the
    /// color, so a rule can reuse a palette color instead of having it dialed in on the sliders.
    /// </summary>
    public class ColorPaletteSelector : HudElementBase {
        public const float ROW_HEIGHT = 26f;
        public const float TOTAL_HEIGHT = ROW_HEIGHT * 2f + SWATCH_SPACING;

        private const int SLOTS_PER_ROW = 7;
        private const float SWATCH_SPACING = 4f;

        private readonly List<Button> _swatches = new List<Button>();

        public ColorPaletteSelector(HudParentBase parent = null) : base(parent) {
            Height = TOTAL_HEIGHT;

            var topRow = CreateRow();
            var bottomRow = CreateRow();

            for (var i = 0; i < SLOTS_PER_ROW * 2; i++) {
                // Highlighting would replace the swatch color on hover and hide the very thing it shows.
                var swatch = new Button() {
                    Color = VRageMath.Color.DimGray,
                    HighlightEnabled = false,
                };

                new BorderBox(swatch) {
                    Color = Style.BorderColor,
                    Thickness = 1f,
                    DimAlignment = DimAlignments.Size,
                };

                var slotIndex = i;
                swatch.MouseInput.LeftClicked += (s, e) => OnSwatchClicked(slotIndex);
                swatch.MouseInput.CursorEntered += (s, e) => HudSoundUtils.PlaySound("HudMouseOver");

                _swatches.Add(swatch);

                if (i < SLOTS_PER_ROW) {
                    topRow.Add(swatch, 1f);
                } else {
                    bottomRow.Add(swatch, 1f);
                }
            }

            var layout = new HudChain(true, this) {
                CollectionContainer = { topRow, bottomRow },
                Spacing = SWATCH_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                DimAlignment = DimAlignments.Width,
                Height = TOTAL_HEIGHT,
            };

            Refresh();
        }

        /// <summary>
        /// Raised with the picked color when a swatch is clicked.
        /// </summary>
        public event Action<VRageMath.Color> ColorPicked;

        private static HudChain CreateRow() {
            return new HudChain(false) {
                Spacing = SWATCH_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                DimAlignment = DimAlignments.Width,
                Height = ROW_HEIGHT,
            };
        }

        /// <summary>
        /// Reads the current build color slots. Slots the player has not defined stay neutral and inert.
        /// </summary>
        public void Refresh() {
            var slots = MyAPIGateway.Session?.LocalHumanPlayer?.BuildColorSlots;

            for (var i = 0; i < _swatches.Count; i++) {
                var hasColor = slots != null && i < slots.Count;

                _swatches[i].Color = hasColor ? ToDisplayColor(slots[i]) : VRageMath.Color.DimGray;
                _swatches[i].InputEnabled = hasColor;
            }
        }

        private void OnSwatchClicked(int slotIndex) {
            var slots = MyAPIGateway.Session?.LocalHumanPlayer?.BuildColorSlots;
            if (slots == null || slotIndex >= slots.Count) {
                return;
            }

            HudSoundUtils.PlaySound("HudMouseClick");
            ColorPicked?.Invoke(ToDisplayColor(slots[slotIndex]));
        }

        /// <summary>
        /// Build color slots are stored as SE color masks, not RGB.
        /// </summary>
        private static VRageMath.Color ToDisplayColor(Vector3 colorMask) {
            Settings.Models.Color color = colorMask;
            return color;
        }
    }
}
