//using RichHudFramework.UI;
//using RichHudFramework.UI.Rendering;
//using Sisk.BuildColors.Settings.Models;
//using VRage.Utils;
//using VRageMath;

//namespace Sisk.BuildColors.UI {
//    public class ColorSetItemElement : HudElementBase {
//        private readonly HudChain _layout;
//        public ColorSetItemElement(ColorSet colorSet, HudParentBase parent = null) : base(parent) {
//            var label = new Label {
//                //DimAlignment = DimAlignments.Width,
//                //ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH,
//                Format = Style.BodyText,
//                Text = colorSet.Name,
//            };

//            var material = new TexturedBox {
//                Material = new Material(MyStringId.Get("Square"), new Vector2(1)),
//                Width = 48,
//                Height = 32
//            };

//            var rowOne = new HudChain(false) {
//                Spacing = 3
//            };

//            var rowTwo = new HudChain(false) {
//                Spacing = 3
//            };

//            for (var i = 0; i < colorSet.Colors.Length; i++) {
//                var color = colorSet.Colors[i];
//                var element = new TexturedBox {
//                    Color = color,

//                    Width = 48,
//                    Height = 32
//                };

//                if (i < 7) {
//                    rowOne.Add(element);
//                } else {
//                    rowTwo.Add(element);
//                }
//            }

//            var verticalLayout = new HudChain(true) {
//                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
//                Size = new Vector2(425, 32),
//                Spacing = 3,

//                CollectionContainer = {
//                    rowOne,
//                    rowTwo
//                }
//            };

//            var horizontalLayout = new HudChain(false) {
//                Spacing = 3,
//                CollectionContainer = {
//                    verticalLayout,
//                    material
//                }
//            };

//            _layout = new HudChain(true, this) {
//                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
//                Size = new Vector2(425, 68),
//                Spacing = 3,
//                MemberMinSize = new Vector2(425, 24f),
//                SizingMode = HudChainSizingModes.ClampChainBoth,
//                CollectionContainer = {
//                    label,
//                    horizontalLayout
//                }
//            };
//        }
//    }
//}