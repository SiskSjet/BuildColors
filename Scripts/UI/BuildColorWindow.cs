using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering;
using Sandbox.ModAPI;
using Sisk.BuildColors.Settings.Models;
using VRageMath;
using Color = VRageMath.Color;

namespace Sisk.BuildColors.UI {
    public class BuildColorWindow : WindowBase {
        private readonly Label _footer;
        private readonly TexturedBox _headerSeperator;
        private readonly LabelBox _testElement;
        private readonly HudChain _content;

        public BuildColorWindow(HudParentBase parent = null) : base(parent) {
            _footer = new Label(body) {
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV,
                Height = 30,
                Padding = new Vector2(10),
                Format = Style.FooterText,
                Text = "by SISK",
            };

            _headerSeperator = new TexturedBox(body) {
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV,
                Padding = new Vector2(65, 0),
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            var colorSetLabel = new Label() {
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
                Text = "Color Sets",
                Format = Style.HeaderText.WithAlignment(TextAlignment.Left),
                Padding = new Vector2(0, 10),
            };

            var seperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
                Padding = new Vector2(0, 50),
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            var colorSetScrollBox = new ScrollBox<ScrollBoxEntry<ColorSetItemElement>, ColorSetItemElement>() {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
                Height = 400,
                AlignVertical = true
            };

            foreach (var colorSet in Mod.Static.ColorSets) {
                colorSetScrollBox.Add(new ColorSetItemElement(colorSet) {
                    DimAlignment = DimAlignments.Width,
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH | ParentAlignments.UsePadding,
                    Height = 75
                });
            }

            _content = new HudChain(true, body) {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                DimAlignment = DimAlignments.Both,
                ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH | ParentAlignments.UsePadding,
                Padding = new Vector2(63, 30),

                CollectionContainer = { colorSetLabel, colorSetScrollBox, seperator }
            };

            header.background.Color = Style.BodyBackgroundColor;
            header.textElement.Offset = new Vector2(0, -10);
            header.Format = Style.HeaderText;
            header.Height = 63;
            HeaderText = Mod.NAME;

            BorderColor = Style.BorderColor;
            BodyColor = Style.BodyBackgroundColor;

            Size = new Vector2(500, 1080);
            Offset = new Vector2(200, 0);
            CanDrag = false;
            AllowResizing = false;
        }


        protected override void Layout() {
            base.Layout();
        }

        protected override void Draw() {
            base.Draw();

            SetOpacity();
        }

        private void SetOpacity() {
            var opacity = MyAPIGateway.Session.Config.UIBkOpacity;

            BorderColor = BorderColor.SetAlphaPct(opacity);
            BodyColor = BodyColor.SetAlphaPct(opacity);
            header.background.Color = BodyColor.SetAlphaPct(opacity);
        }
    }

    public class ColorSetItemElement : HudElementBase {
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
        }

        public string ColorSet { get; set; }
    }
}
