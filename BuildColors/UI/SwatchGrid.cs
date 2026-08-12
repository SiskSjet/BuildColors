using RichHudFramework.UI;
using Sisk.BuildColors.Settings.Models;
using System;
using System.Collections.Generic;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// The fourteen build color slots, two rows of seven in slot order.
    /// </summary>
    internal class SwatchGrid : HudElementBase {
        public const float SPACING = 4f;

        /// <summary>
        /// Tallest a row is allowed to get.
        /// </summary>
        public const float MAX_ROW_HEIGHT = 48f;

        /// <summary>
        /// Shortest a row may be squeezed to before the colors stop being distinguishable.
        /// </summary>
        public const float MIN_ROW_HEIGHT = 18f;

        private const int SLOTS_PER_ROW = 7;

        private readonly List<BorderBox> _borders = new List<BorderBox>(ColorSet.SLOTS);
        private readonly List<Button> _swatches = new List<Button>(ColorSet.SLOTS);
        private bool[] _marked;

        public SwatchGrid(float width, float rowHeight, bool interactive = false, HudParentBase parent = null) : base(parent) {
            rowHeight = MathHelper.Clamp(Math.Min(rowHeight, SlotWidth(width)), MIN_ROW_HEIGHT, MAX_ROW_HEIGHT);

            Size = new Vector2(width, HeightFor(rowHeight));

            var topRow = CreateRow(width, rowHeight);
            var bottomRow = CreateRow(width, rowHeight);

            for (var i = 0; i < ColorSet.SLOTS; i++) {
                var swatch = new Button() {
                    Color = Style.SunkenBackgroundColor,

                    HighlightEnabled = false,
                    InputEnabled = interactive,
                };

                var border = new BorderBox(swatch) {
                    DimAlignment = DimAlignments.Both,
                    Color = Style.SubtleBorderColor,
                    Thickness = 1f,
                };

                if (interactive) {
                    var index = i;
                    swatch.MouseInput.LeftClicked += (sender, args) => OnClicked(index);
                    swatch.MouseInput.RightClicked += (sender, args) => OnRightClicked(index);
                    swatch.MouseInput.CursorEntered += (sender, args) => HudSoundUtils.PlaySound("HudMouseOver");
                }

                _swatches.Add(swatch);
                _borders.Add(border);

                if (i < SLOTS_PER_ROW) {
                    topRow.Add(swatch, 1f);
                } else {
                    bottomRow.Add(swatch, 1f);
                }
            }

            var layout = new HudChain(true, this) {
                CollectionContainer = { topRow, bottomRow },
                Spacing = SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = Size.Y,
            };
        }

        /// <summary>
        /// Raised with the slot index when a swatch is clicked.
        /// </summary>
        public event Action<int> SlotClicked;

        /// <summary>
        /// Raised with the slot index when a swatch is right clicked.
        /// </summary>
        public event Action<int> SlotRightClicked;

        public static float HeightFor(float rowHeight) {
            return rowHeight * 2f + SPACING;
        }

        /// <summary>
        /// Width one swatch gets. A row never grows past it, so swatches are never stood on end.
        /// </summary>
        public static float SlotWidth(float width) {
            return (width - SPACING * (SLOTS_PER_ROW - 1)) / SLOTS_PER_ROW;
        }

        /// <summary>
        /// Row height that fits the given number of grids into the space left for them.
        /// </summary>
        public static float FitRowHeight(float available, int gridCount) {
            var perGrid = available / Math.Max(gridCount, 1);

            return MathHelper.Clamp((perGrid - SPACING) * .5f, MIN_ROW_HEIGHT, MAX_ROW_HEIGHT);
        }

        public void SetColorSet(ColorSet colorSet) {
            SetMasks(colorSet.Upgraded().Masks);
        }

        public void SetMasks(ColorMask[] masks) {
            for (var i = 0; i < _swatches.Count; i++) {
                if (masks != null && i < masks.Length) {
                    _swatches[i].Color = masks[i].ToDisplayColor();
                } else {
                    _swatches[i].Color = Style.SunkenBackgroundColor;
                }
            }

            RefreshBorders();
        }

        /// <summary>
        /// Marks which slots are held back from the next roll.
        /// </summary>
        public void SetLocked(bool[] locked) {
            _marked = locked;

            RefreshBorders();
        }

        /// <summary>
        /// Marks the one slot that is currently chosen.
        /// </summary>
        public void SetSelected(int index) {
            var marks = new bool[ColorSet.SLOTS];

            if (index >= 0 && index < marks.Length) {
                marks[index] = true;
            }

            SetLocked(marks);
        }

        public void Clear() {
            SetMasks(null);
        }

        private static HudChain CreateRow(float width, float height) {
            return new HudChain(false) {
                Spacing = SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = height,
            };
        }

        /// <summary>
        /// Draws a marked slot with an accent border.
        /// </summary>
        private void RefreshBorders() {
            for (var i = 0; i < _borders.Count; i++) {
                var isMarked = _marked != null && i < _marked.Length && _marked[i];

                _borders[i].Color = isMarked ? Style.AccentColor : Style.SubtleBorderColor;
                _borders[i].Thickness = isMarked ? 2f : 1f;
            }
        }

        private void OnClicked(int index) {
            HudSoundUtils.PlaySound("HudMouseClick");
            SlotClicked?.Invoke(index);
        }

        private void OnRightClicked(int index) {
            HudSoundUtils.PlaySound("HudMouseClick");
            SlotRightClicked?.Invoke(index);
        }
    }
}
