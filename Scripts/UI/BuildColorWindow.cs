using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using Sisk.BuildColors.Settings.Models;
using System;
using System.Linq;
using VRageMath;

namespace Sisk.BuildColors.UI {

    public class BuildColorWindow : WindowBase {
        private readonly ColorPickerHSV _baseColorPicker;
        private readonly ListBox<ColorSet> _colorSetList;
        private readonly ColorSetElement _colorSetPreview;
        private readonly BorderedButton _loadButton;
        private readonly Dropdown<ColorSchemeGenerator.Preset> _presetDropdown;
        private readonly BorderedButton _removeButton;
        private readonly Dropdown<ColorSchemeGenerator.Scheme> _schemeDropdown;
        private readonly ColorSchemeGenerator _schemeGenerator;
        private readonly ColorSetElement _schemePreview;

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

            _colorSetPreview = new ColorSetElement();

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
                CollectionContainer = { saveButton },
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

            var schemeSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            var schemeLabel = new Label() {
                Text = "Color Scheme",
            };

            _schemeDropdown = new Dropdown<ColorSchemeGenerator.Scheme>();
            foreach (var scheme in Enum.GetValues(typeof(ColorSchemeGenerator.Scheme)).Cast<ColorSchemeGenerator.Scheme>()) {
                _schemeDropdown.Add(scheme.ToString(), scheme);
            }

            _schemeDropdown.SetSelection(ColorSchemeGenerator.Scheme.Complementary);

            var schemeLayout = new HudChain(false) {
                CollectionContainer = { schemeLabel, _schemeDropdown },
                Spacing = 8f,
            };

            var presetLabel = new Label() {
                Text = "Color Presets",
            };

            _presetDropdown = new Dropdown<ColorSchemeGenerator.Preset>();
            foreach (var preset in Enum.GetValues(typeof(ColorSchemeGenerator.Preset)).Cast<ColorSchemeGenerator.Preset>()) {
                _presetDropdown.Add(preset.ToString(), preset);
            }

            _presetDropdown.SetSelection(ColorSchemeGenerator.Preset.Default);

            var presetLayout = new HudChain(false) {
                CollectionContainer = { presetLabel, _presetDropdown },
                Spacing = 8f,
            };

            _baseColorPicker = new ColorPickerHSV() {
                Name = "Base color",
            };

            var generateButton = new BorderedButton() { Text = "Generate", Padding = Vector2.Zero };
            _schemePreview = new ColorSetElement();

            var bodyLayout = new HudChain(true) {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV,
                CollectionContainer = { headerSeperator, _colorSetPreview, previewSeperator, colorSetLabel, _colorSetList, controls, schemeSeperator, schemeLayout, presetLayout, _baseColorPicker, generateButton, _schemePreview },
                Spacing = 10f,
            };

            bodyLayout.Register(body);

            _schemeGenerator = new ColorSchemeGenerator();

            _colorSetList.SelectionChanged += OnColorSetChanged;
            _loadButton.MouseInput.LeftClicked += OnLoadClicked;
            _removeButton.MouseInput.LeftClicked += OnRemoveClicked;
            saveButton.MouseInput.LeftClicked += OnSaveClicked;
            _schemeDropdown.SelectionChanged += OnSchemeChanged;
            _presetDropdown.SelectionChanged += OnPresetChanged;
            generateButton.MouseInput.LeftClicked += OnGenerateClicked;
            LoadColorSets();
            OnColorSetChanged(null, null);
        }

        public void LoadColorSets() {
            _colorSetList.ClearEntries();
            foreach (var colorSet in Mod.Static.ColorSets) {
                _colorSetList.Add(colorSet.Name, colorSet);
            }
        }

        protected override void Draw() {
            base.Draw();

            SetOpacity();
        }

        private void GenerateColorSet(bool reuseColor = false) {
            var generator = _schemeGenerator;

            var preset = _presetDropdown.Selection.AssocMember;
            var scheme = _schemeDropdown.Selection.AssocMember;

            var result = reuseColor ? generator.Generate(baseColor: generator.BaseColor, preset: preset, scheme: scheme) : generator.Generate(preset: preset, scheme: scheme);

            // convert result to Color array.
            var colors = result.Select(x => (Settings.Models.Color)x).ToArray();
            var colorSet = new ColorSet("Generated", colors);
            _schemePreview.SetColorSet(colorSet);
            //Mod.Static.ColorSets.Add(colorSet);
            //LoadColorSets();
        }

        private void OnColorSetChanged(object sender, EventArgs e) {
            if (_colorSetList.Selection != null) {
                var selection = _colorSetList.Selection.AssocMember;
                _colorSetPreview.SetColorSet(selection);
                _loadButton.InputEnabled = true;
                _removeButton.InputEnabled = true;
            } else {
                _loadButton.InputEnabled = false;
                _removeButton.InputEnabled = false;
            }
        }

        private void OnGenerateClicked(object sender, EventArgs e) {
            GenerateColorSet();
        }

        private void OnLoadClicked(object sender, EventArgs e) {
            if (_colorSetList.Selection != null) {
                var selection = _colorSetList.Selection.AssocMember;
                Mod.Static.LoadColorSet(selection.Name);
            }
        }

        private void OnPresetChanged(object sender, EventArgs e) {
            GenerateColorSet(true);
        }

        private void OnRemoveClicked(object sender, EventArgs e) {
            if (_colorSetList.Selection != null) {
                var selection = _colorSetList.Selection.AssocMember;
                Mod.Static.RemoveColorSet(selection.Name);
                LoadColorSets();
            }
        }

        private void OnSaveClicked(object sender, EventArgs e) {
            var window = new SaveWindow();
            window.Register(HudMain.HighDpiRoot);
            window.SaveClicked += (s, args) => {
                LoadColorSets();
            };
        }

        private void OnSchemeChanged(object sender, EventArgs e) {
            GenerateColorSet(true);
        }

        private void SetOpacity() {
            var opacity = MyAPIGateway.Session.Config.UIBkOpacity;

            BorderColor = BorderColor.SetAlphaPct(opacity);
            BodyColor = BodyColor.SetAlphaPct(opacity);
            header.background.Color = BodyColor.SetAlphaPct(opacity);
        }
    }
}