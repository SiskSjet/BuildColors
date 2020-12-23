using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using Sisk.BuildColors.Settings.Models;
using VRage.Utils;
using VRageMath;

// ReSharper disable ArrangeAccessorOwnerBody

namespace Sisk.BuildColors.UI {
    internal sealed class ColorSetItem : HudElementBase, IListBoxEntry {
        private readonly HudChain<TexturedBox> _colorSetRow1;
        private readonly HudChain<TexturedBox> _colorSetRow2;
        private readonly HudChain<HudElementBase> _horizontalLayout;

        private readonly HudChain<HudElementBase> _layout;
        private readonly TexturedBox _materialBox;
        private readonly Label _name;
        private readonly HudChain<HudChain<TexturedBox>> _verticalLayout;

        public ColorSetItem(ColorSet colorSet, IHudParent parent = null) : base(parent) {
            _name = new Label {
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH,
                Format = Style.BodyText,
                Text = colorSet.Name,
            };

            _materialBox = new TexturedBox {
                Material = new Material(MyStringId.Get("Square"), new Vector2(1)),
                Width = 48,
                Height = 32
            };

            _colorSetRow1 = new HudChain<TexturedBox> {
                Spacing = 3
            };

            _colorSetRow2 = new HudChain<TexturedBox> {
                Spacing = 3
            };

            for (var i = 0; i < colorSet.Colors.Length; i++) {
                var color = colorSet.Colors[i];
                var element = new TexturedBox {
                    Color = color,

                    Width = 48,
                    Height = 32
                };

                if (i < 7) {
                    _colorSetRow1.Add(element);
                } else {
                    _colorSetRow2.Add(element);
                }
            }

            _verticalLayout = new HudChain<HudChain<TexturedBox>> {
                AlignVertical = true,
                Spacing = 3,
                ChildContainer = {
                    _colorSetRow1,
                    _colorSetRow2
                }
            };

            _horizontalLayout = new HudChain<HudElementBase> {
                Spacing = 3,
                ChildContainer = {
                    _verticalLayout,
                    _materialBox
                }
            };

            _layout = new HudChain<HudElementBase>(this) {
                AlignVertical = true,
                Spacing = 3,
                ChildContainer = {
                    _name,
                    _horizontalLayout
                }
            };
        }

        public override float Width {
            get { return _layout.Width; }
            set { }
        }

        public override float Height {
            get { return _layout.Height; }
            set { }
        }

        public override bool Visible {
            get { return base.Visible && Enabled; }
        }

        public bool Enabled { get; set; }

        public void Reset() {
            _name.TextBoard.Clear();
        }
    }
}