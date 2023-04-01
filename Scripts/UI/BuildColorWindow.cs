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
                AlignVertical = true,
            };

            var colorSetListBox = new ListBox<ColorSet>();

            foreach (var colorSet in Mod.Static.ColorSets) {
                colorSetListBox.Add(colorSet.Name, colorSet);
                colorSetScrollBox.Add(new ColorSetItemElement(colorSet));
            }

            var loadButton = new BorderedButton() { Text = "Load", Padding = Vector2.Zero };
            var renameButton = new BorderedButton() { Text = "Rename", Padding = Vector2.Zero };
            var saveButton = new BorderedButton() { Text = "Save", Padding = Vector2.Zero };
            var deleteButton = new BorderedButton() { Text = "Delete", Padding = Vector2.Zero };

            var controls = new HudChain(false) {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                CollectionContainer = { loadButton, renameButton, saveButton, deleteButton },
                Spacing = 8f,
            };

            _content = new HudChain(true, body) {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                DimAlignment = DimAlignments.Both,
                ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH | ParentAlignments.UsePadding,
                Padding = new Vector2(63, 30),
                Spacing = 8f,
                Size = new Vector2(500, 1080),
                CollectionContainer = { colorSetLabel, colorSetScrollBox, colorSetListBox, seperator, controls }
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
}
