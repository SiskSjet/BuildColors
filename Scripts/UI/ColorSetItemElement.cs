using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
using Sisk.BuildColors.Settings.Models;
using VRageMath;

namespace Sisk.BuildColors.UI {
    public class ColorSetItemElement : HudElementBase, IMinLabelElement {
        private readonly ColorSet _colorSet;

        public ColorSetItemElement(ColorSet colorSet, HudParentBase parent = null) : base(parent) {
            _colorSet = colorSet;


            var title = new Label() {
                Text = _colorSet.Name,
                ParentAlignment = ParentAlignments.Left
            };
            var row1 = new HudChain(false) {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                Height = 25
            };

            var row2 = new HudChain(false) {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                Height = 25
            };

            var vertical = new HudChain(true, this) {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                DimAlignment = DimAlignments.Both,
                ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH | ParentAlignments.UsePadding,
                Padding = new Vector2(5),
                CollectionContainer = { title, row1, row2 }
            };

            for (var i = 0; i < colorSet.Colors.Length; i++) {
                var color = colorSet.Colors[i];
                var element = new TexturedBox {
                    Color = color,
                    Padding = new Vector2(5),
                };

                if (i < 7) {
                    row1.Add(element);
                } else {
                    row2.Add(element);
                }
            }

            DimAlignment = DimAlignments.Width;
            ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH | ParentAlignments.UsePadding;
            Height = 75;

            TextBoard = new TextBoard();
        }

        public string ColorSet { get; set; }

        public ITextBoard TextBoard { get; }
    }
}
