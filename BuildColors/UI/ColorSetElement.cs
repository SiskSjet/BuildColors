using RichHudFramework.UI;
using Sisk.BuildColors.Settings.Models;
using System.Collections.Generic;

namespace Sisk.BuildColors.UI {

    public class ColorSetElement : HudElementBase {
        private readonly HudChain _layout;
        private readonly List<TexturedBox> _textures = new List<TexturedBox>();
        private ColorSet _colorSet;

        public ColorSetElement(HudParentBase parent = null) : base(parent) {
            DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding;

            var title = new Label() {
                Text = _colorSet.Name,
                ParentAlignment = ParentAlignments.Left
            };
            var row1 = new HudChain(false) {
                SizingMode = HudChainSizingModes.ClampChainAlignAxis | HudChainSizingModes.FitMembersOffAxis,
                Spacing = 4f,
                Height = 40,
            };

            var row2 = new HudChain(false) {
                SizingMode = HudChainSizingModes.ClampChainAlignAxis | HudChainSizingModes.FitMembersOffAxis,
                Spacing = 4f,
                Height = 40,
            };

            _layout = new HudChain(true) {
                CollectionContainer = { title, row1, row2 },
                Spacing = 4f,
                Height = 60
            };

            _layout.Register(this);

            for (var i = 0; i < 14; i++) {
                var element = new TexturedBox {
                    Color = VRageMath.Color.White,
                    DimAlignment = DimAlignments.Height,
                    Width = 60,
                };

                _textures.Add(element);

                if (i < 7) {
                    row1.Add(element);
                } else {
                    row2.Add(element);
                }
            }
        }

        public ColorSet ColorSet {
            get { return _colorSet; }
        }

        public void SetColorSet(ColorSet colorSet) {
            _colorSet = colorSet;

            if (colorSet.Equals(default(ColorSet))) {
                return;
            }

            for (var i = 0; i < _textures.Count && i < colorSet.Colors.Length; i++) {
                _textures[i].Color = colorSet.Colors[i];
            }
        }

        protected override void Layout() {
            base.Layout();

            Height = 0;
            foreach (var item in _layout) {
                Height += item.Element.Height;
            }
        }
    }
}