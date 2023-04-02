using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
using Sisk.BuildColors.Settings.Models;
using System.Collections.Generic;
using System.Xml.Linq;
using VRageMath;

namespace Sisk.BuildColors.UI {
    public class ColorSetElement : HudElementBase {
        private ColorSet _colorSet;
        private readonly List<TexturedBox> _textures = new List<TexturedBox>();

        public ColorSetElement(HudParentBase parent = null) : base(parent) {
            var title = new Label() {
                Text = _colorSet.Name,
                ParentAlignment = ParentAlignments.Left
            };
            var row1 = new HudChain(false) {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                Height = 40
            };

            var row2 = new HudChain(false) {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                Height = 40
            };

            var vertical = new HudChain(true, this) {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                DimAlignment = DimAlignments.Both,
                ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH | ParentAlignments.UsePadding,
                Padding = new Vector2(5),
                CollectionContainer = { title, row1, row2 }
            };

            for (var i = 0; i < 14; i++) {
                var element = new TexturedBox {
                    Color = VRageMath.Color.White,
                    Padding = new Vector2(5),
                };

                _textures.Add(element);

                if (i < 7) {
                    row1.Add(element);
                } else {
                    row2.Add(element);
                }
            }

            DimAlignment = DimAlignments.Width;
            Height = 75;
        }

        public string ColorSet { get; set; }
        public void SetColorSet(ColorSet colorSet) {
            _colorSet = colorSet;

            if (colorSet.Equals(default(ColorSet))) {
                return;
            }

            for (var i = 0; i < _textures.Count; i++) {
                _textures[i].Color = colorSet.Colors[i];
            }
        }
    }
}
