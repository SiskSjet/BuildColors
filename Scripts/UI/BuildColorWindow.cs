﻿using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using Sisk.BuildColors.Settings.Models;
using System;
using System.Linq;
using VRageMath;

namespace Sisk.BuildColors.UI {
    public class BuildColorWindow : WindowBase {
        private readonly ListBox<ColorSet> _colorSetList;
        private readonly BorderedButton _loadButton;
        private readonly ColorSetElement _preview;
        private readonly BorderedButton _removeButton;

        public BuildColorWindow(HudParentBase parent = null) : base(parent) {
            BorderColor = Style.BorderColor;
            BodyColor = Style.BodyBackgroundColor;

            header.background.Color = Style.BodyBackgroundColor;
            header.textElement.Offset = new Vector2(0, -10);
            header.Format = Style.HeaderText;
            header.Height = 63;
            HeaderText = Mod.NAME;
            ZOffset = 1;

            Size = new Vector2(500, 1080);
            Offset = new Vector2(200, 0);

            CanDrag = false;
            AllowResizing = false;

            var headerSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            var colorSetLabel = new Label() {
                ParentAlignment = ParentAlignments.Left,
                DimAlignment = DimAlignments.Width,
                Text = "Color Sets",
                Format = Style.HeaderText.WithAlignment(TextAlignment.Left),
                //Padding = new Vector2(0, 10),
            };

            _preview = new ColorSetElement();

            var previewSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            _colorSetList = new ListBox<ColorSet>() {
                DimAlignment = DimAlignments.Width,
            };

            _loadButton = new BorderedButton() { Text = "Load", Padding = Vector2.Zero };
            var saveButton = new BorderedButton() { Text = "Save", Padding = Vector2.Zero };
            _removeButton = new BorderedButton() { Text = "Remove", Padding = Vector2.Zero };

            var buttonRow1 = new HudChain(false) {
                CollectionContainer = { _loadButton, _removeButton },
                Spacing = 8f,
            };

            var buttonRow2 = new HudChain(false) {
                CollectionContainer = { saveButton  },
                Spacing = 8f,
            };

            var buttonSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            var controls = new HudChain(true) {
                CollectionContainer = { buttonRow1, buttonSeperator, buttonRow2 },
                Spacing = 10f,
            };

            var bodyLayout = new HudChain(true) {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV,
                CollectionContainer = { headerSeperator, _preview, previewSeperator, colorSetLabel, _colorSetList, controls },
                Spacing = 10f,
            };

            bodyLayout.Register(body);

            _colorSetList.SelectionChanged += OnColorSetChanged;
            _loadButton.MouseInput.LeftClicked += OnLoadClicked;
            _removeButton.MouseInput.LeftClicked += OnRemoveClicked;
            saveButton.MouseInput.LeftClicked += OnSaveClicked;
            LoadColorSets();
            OnColorSetChanged(null, null);
        }

        private void OnSaveClicked(object sender, EventArgs e) {
            var window = new SaveWindow();
            window.Register(HudMain.HighDpiRoot);
            window.SaveClicked += (s, args) => {
                LoadColorSets();
            };
        }

        private void OnRemoveClicked(object sender, EventArgs e) {
            if (_colorSetList.Selection != null) {
                var selection = _colorSetList.Selection.AssocMember;
                Mod.Static.RemoveColorSet(selection.Name);
                LoadColorSets();
            }
        }

        private void OnLoadClicked(object sender, EventArgs e) {
            if (_colorSetList.Selection != null) {
                var selection = _colorSetList.Selection.AssocMember;
                Mod.Static.LoadColorSet(selection.Name);
            }
        }

        public void LoadColorSets() {
            _colorSetList.ClearEntries();
            foreach (var colorSet in Mod.Static.ColorSets) {
                _colorSetList.Add(colorSet.Name, colorSet);
            }
        }

        private void OnColorSetChanged(object sender, EventArgs e) {
            if (_colorSetList.Selection != null) {
                var selection = _colorSetList.Selection.AssocMember;
                _preview.SetColorSet(selection);
                _loadButton.InputEnabled = true;
                _removeButton.InputEnabled = true;
            } else {
                _loadButton.InputEnabled = false;
                _removeButton.InputEnabled = false;
            }
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