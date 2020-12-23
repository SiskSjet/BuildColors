using System;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using Sisk.BuildColors.Settings.Models;
using VRageMath;
using Color = VRageMath.Color;

namespace Sisk.BuildColors.UI {
    internal class ColorSets : ScrollBox<ColorSetItem> {
        private readonly TexturedBox _selectionBox;
        private readonly TexturedBox _tab;

        private int _index;

        public ColorSets(IHudParent parent = null) : base(parent) {
            DimAlignment = DimAlignments.Width;
            AlignVertical = true;
            SizingMode = ScrollBoxSizingModes.None;
            Color = Color.Transparent;
            MinimumVisCount = 5;
            MinimumSize = new Vector2(350, 0f);
            Spacing = 3;

            scrollBar.Padding = new Vector2(12f, 16f);
            scrollBar.Width = 8f;
            List.AutoResize = false;

            _selectionBox = new TexturedBox(List) {
                Color = Style.SelectionBackgroundColor,
                //Padding = new Vector2(30f, 0f),
                ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH
            };

            _tab = new TexturedBox(_selectionBox) {
                Width = 3f,
                //Offset = new Vector2(15f, 0f),
                Color = new Color(142, 188, 206),
                ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH
            };

            AddMembers();
            CaptureCursor = true;
        }

        public ColorSetItem Selection => _index < List.Count ? List[_index] : null;

        protected override void Draw() {
            if (Selection != null) {
                _selectionBox.Color = Style.SelectionBackgroundColor;
                _selectionBox.Size = new Vector2(Width - divider.Width - scrollBar.Width, Selection.Size.Y + 2f * Scale);
                _selectionBox.Offset = new Vector2(-22f * Scale, Selection.Offset.Y - 1f * Scale);
                _tab.Height = _selectionBox.Height;
            }
        }

        protected override void HandleInput() {
            if (IsMousedOver) {
                var delta = MyAPIGateway.Input.DeltaMouseScrollWheelValue();
                if (delta > 0) {
                    _index = Math.Max(0, Math.Min(List.Count - 1, _index - 1));
                    if (_index <= scrollBar.Current - 1) {
                        scrollBar.Current = Start - 1;
                    }
                } else if (delta < 0) {
                    _index = Math.Max(0, Math.Min(List.Count - 1, _index + 1));
                    if (_index >= scrollBar.Max - 1) {
                        scrollBar.Current = Start + 1;
                    }
                }

                if (SharedBinds.LeftButton.IsReleased) {
                    for (var index = 0; index < List.Count; index++) {
                        var item = List[index];
                        if (item.IsMousedOver) {
                            _index = index;
                            break;
                        }
                    }
                }
            }
        }

        private void AddMember(ColorSet colorSet, int index) {
            var propBox = new ColorSetItem(colorSet) {
                ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH,
                CaptureCursor = true
            };

            AddToList(propBox);
            List[index].Enabled = true;
        }

        private void AddMembers() {
            var index = 0;
            foreach (var colorSet in Mod.Static.ColorSets) {
                AddMember(colorSet, index);
                index++;
            }
        }
    }
}