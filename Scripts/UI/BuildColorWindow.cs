using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.ColorSpace;
using System;
using System.Linq;
using VRageMath;
using EventHandler = Sisk.BuildColors.EventHandler;

namespace Sisk.BuildColors.UI {

    public class BuildColorWindow : WindowBase {
        public const float ASPECT_RATIO_END = 5f / 4f;
        public const float ASPECT_RATIO_START = 16f / 9f;
        public const float OFFSET_END = -60;
        public const float OFFSET_START = 200;
        public const float SLOPE = (OFFSET_END - OFFSET_START) / (ASPECT_RATIO_END - ASPECT_RATIO_START);
        public const float Y_INTERCEPT = OFFSET_START - SLOPE * ASPECT_RATIO_START;
        private readonly ColorPickerHSV2 _baseColorPicker;
        private readonly ListBox<ColorSet> _colorsetList;
        private readonly ColorSetElement _colorsetPreview;
        private readonly BorderedButton _generateColorSchemeButton;
        private readonly BorderedButton _loadColorsetButton;
        private readonly Dropdown<ColorSchemeGenerator.Preset> _presetDropdown;
        private readonly BorderedButton _removeColorsetButton;
        private readonly Dropdown<ColorSchemeGenerator.Scheme> _schemeDropdown;
        private readonly ColorSchemeGenerator _schemeGenerator;
        private readonly ColorSetElement _schemePreview;
        private DateTime _lastClickTime;
        private string _lastSelectedName;

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
            Offset = new Vector2(OFFSET_START, 0);

            CanDrag = false;
            AllowResizing = false;

            var headerSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            _colorsetPreview = new ColorSetElement();

            var colorsetListSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            var colorsetLabel = new Label() {
                ParentAlignment = ParentAlignments.Left,
                Text = "Color Sets",
            };

            _colorsetList = new ListBox<ColorSet>() {
                DimAlignment = DimAlignments.Width,
            };

            _loadColorsetButton = new BorderedButton() {
                Text = "Load",
                Padding = Vector2.Zero
            };
            _removeColorsetButton = new BorderedButton() {
                Text = "Remove",
                Padding = Vector2.Zero
            };
            var saveActivColorsButton = new BorderedButton() {
                ParentAlignment = ParentAlignments.Right,
                Text = "Save (Color Picker)",
                Padding = Vector2.Zero
            };

            var buttonRow1 = new HudChain(false) {
                CollectionContainer = { _loadColorsetButton, _removeColorsetButton },
                Spacing = 8f,
            };

            var colorsetPreviewSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            var colorsetControls = new HudChain(true) {
                CollectionContainer = { buttonRow1, saveActivColorsButton },
                Spacing = 10f,
            };

            var colorsetSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            var colorSchemeGeneratorLabel = new Label() {
                ParentAlignment = ParentAlignments.Left,
                Text = "Color Scheme Generator",
            };

            var schemeLabel = new Label() {
                ParentAlignment = ParentAlignments.Left,
                Text = "Color Scheme",
            };

            _schemeDropdown = new Dropdown<ColorSchemeGenerator.Scheme>() {
                ParentAlignment = ParentAlignments.Left,
                Width = 220,
            };
            foreach (var scheme in Enum.GetValues(typeof(ColorSchemeGenerator.Scheme)).Cast<ColorSchemeGenerator.Scheme>()) {
                _schemeDropdown.Add(scheme.ToString(), scheme);
            }

            _schemeDropdown.SetSelection(ColorSchemeGenerator.Scheme.Default);

            var schemeLayout = new HudChain(true) {
                CollectionContainer = { schemeLabel, _schemeDropdown },
                Spacing = 8f,
            };

            var presetLabel = new Label() {
                ParentAlignment = ParentAlignments.Left,
                Text = "Color Presets",
            };

            _presetDropdown = new Dropdown<ColorSchemeGenerator.Preset>() {
                ParentAlignment = ParentAlignments.Left,
                Width = 220,
            };
            foreach (var preset in Enum.GetValues(typeof(ColorSchemeGenerator.Preset)).Cast<ColorSchemeGenerator.Preset>()) {
                _presetDropdown.Add(preset.ToString(), preset);
            }

            _presetDropdown.SetSelection(ColorSchemeGenerator.Preset.None);

            var presetLayout = new HudChain(true) {
                CollectionContainer = { presetLabel, _presetDropdown },
                Spacing = 8f,
            };

            var schemePresetLayout = new HudChain(false) {
                CollectionContainer = { schemeLayout, presetLayout },
                Spacing = 8f,
            };

            var randomColorLabel = new Label() {
                ParentAlignment = ParentAlignments.Left,
                Text = "Random base color"
            };

            var randomColorCheckbox = new BorderedCheckBox() {
                ParentAlignment = ParentAlignments.Right,
                IsBoxChecked = true,
            };

            var randomColorLayout = new HudChain(false) {
                ParentAlignment = ParentAlignments.Left,
                CollectionContainer = { randomColorLabel, randomColorCheckbox },
                Spacing = 8f,
            };

            _baseColorPicker = new ColorPickerHSV2() {
                ParentAlignment = ParentAlignments.Left,
                DimAlignment = DimAlignments.Width,
                Name = "Base color",
                Visible = false,
            };

            _generateColorSchemeButton = new BorderedButton() {
                Text = "Generate",
                Padding = Vector2.Zero
            };

            var saveGeneratedColorSchemeButton = new BorderedButton() {
                Text = "Save",
                Padding = Vector2.Zero
            };

            var generatorControls = new HudChain(false) {
                CollectionContainer = { _generateColorSchemeButton, saveGeneratedColorSchemeButton },
                Spacing = 8f,
            };

            var generatorOptionsSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            var generatorControlsSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            _schemePreview = new ColorSetElement();

            var colorsetLayout = new HudChain(true) {
                CollectionContainer = { colorsetLabel, _colorsetList, colorsetListSeperator, _colorsetPreview, colorsetPreviewSeperator, colorsetControls },
                Spacing = 10f,
            };

            var generatorLayout = new HudChain(true) {
                CollectionContainer = { colorSchemeGeneratorLabel, schemePresetLayout, randomColorLayout, _baseColorPicker, generatorOptionsSeperator, generatorControls, generatorControlsSeperator, _schemePreview },
                Spacing = 10f,
            };

            var bodyLayout = new HudChain(true) {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV,
                CollectionContainer = { headerSeperator, colorsetLayout, colorsetSeperator, generatorLayout },
                Spacing = 10f,
            };

            bodyLayout.Register(body);

            _schemeGenerator = new ColorSchemeGenerator();

            randomColorCheckbox.MouseInput.LeftClicked += OnRandomColorChanged;
            _baseColorPicker.ColorChanged += OnBaseColorChanged;

            _colorsetList.SelectionChanged += OnColorSetChanged;
            _colorsetList.MouseInput.CursorEntered += OnMouseOver;

            _loadColorsetButton.MouseInput.LeftClicked += OnLoadClicked;
            _loadColorsetButton.MouseInput.CursorEntered += OnMouseOver;

            _removeColorsetButton.MouseInput.LeftClicked += OnRemoveClicked;
            _removeColorsetButton.MouseInput.CursorEntered += OnMouseOver;

            saveActivColorsButton.MouseInput.LeftClicked += OnSaveActiveColorsClicked;
            saveActivColorsButton.MouseInput.CursorEntered += OnMouseOver;

            _schemeDropdown.SelectionChanged += OnSchemeChanged;
            _schemeDropdown.MouseInput.CursorEntered += OnMouseOver;

            _presetDropdown.SelectionChanged += OnPresetChanged;
            _presetDropdown.MouseInput.CursorEntered += OnMouseOver;

            _generateColorSchemeButton.MouseInput.LeftClicked += OnGenerateColorSchemeClicked;
            _generateColorSchemeButton.MouseInput.CursorEntered += OnMouseOver;

            saveGeneratedColorSchemeButton.MouseInput.LeftClicked += OnSaveGeneratedColorSchemeClicked;
            saveGeneratedColorSchemeButton.MouseInput.CursorEntered += OnMouseOver;

            randomColorCheckbox.MouseInput.CursorEntered += OnMouseOver;
            randomColorCheckbox.MouseInput.LeftClicked += (s, e) => HudSoundUtils.PlaySound("HudMouseClick");

            foreach (var item in _baseColorPicker.sliders) {
                item.MouseInput.CursorEntered += OnMouseOver;
            }

            GenerateColorSet();
            LoadColorSets();
        }

        public void LoadColorSets() {
            _colorsetList.ClearEntries();
            foreach (var colorSet in Mod.Static.ColorSets) {
                _colorsetList.Add(colorSet.Name, colorSet);
            }

            if (_colorsetList.Count > 0) {
                _colorsetList.SetSelectionAt(0);
            }
        }

        protected override void Draw() {
            base.Draw();

            SetOpacity();
        }

        protected override void Layout() {
            base.Layout();

            var screenWidth = MyAPIGateway.Session.Camera.ViewportSize.X;
            var screenHeight = MyAPIGateway.Session.Camera.ViewportSize.Y;
            var aspectRatio = screenWidth / screenHeight;

            var offset = SLOPE * aspectRatio + Y_INTERCEPT;
            Offset = new Vector2(offset, 0f);
        }

        private void GenerateColorSet(bool reuseColor = false) {
            var generator = _schemeGenerator;

            var preset = _presetDropdown.Selection.AssocMember;
            var scheme = _schemeDropdown.Selection.AssocMember;

            var useDefinedBaseColor = _baseColorPicker.Visible;

            var color = useDefinedBaseColor
                ? new HSV(_baseColorPicker.Color.X, _baseColorPicker.Color.Y / 100f, _baseColorPicker.Color.Z / 100f).ToHSL()
                : reuseColor ? generator.BaseColor : (HSL?)null;

            var result = generator.Generate(color: color, preset: preset, scheme: scheme);

            var baseColor = generator.BaseColor.ToHSV();
            if (!useDefinedBaseColor) {
                _baseColorPicker.Color = new Vector3(baseColor.H, baseColor.S * 100, baseColor.V * 100);
            }

            // convert result to Color array.
            var colors = result.Select(x => (Settings.Models.Color)x).ToArray();
            var colorSet = new ColorSet("Generated", colors);
            _schemePreview.SetColorSet(colorSet);
        }

        private void OnBaseColorChanged(object sender, EventArgs e) {
            GenerateColorSet();
        }

        private void OnColorSetChanged(object sender, EventArgs e) {
            if (_colorsetList.Selection != null) {
                var selection = _colorsetList.Selection.AssocMember;
                _colorsetPreview.SetColorSet(selection);
                _loadColorsetButton.InputEnabled = true;
                _removeColorsetButton.InputEnabled = true;
                HudSoundUtils.PlaySound("HudMouseClick");

                var time = DateTime.Now;
                if (time - _lastClickTime < TimeSpan.FromMilliseconds(500) && selection.Name == _lastSelectedName) {
                    Mod.Static.LoadColorSet(selection.Name);
                    HudSoundUtils.PlaySound("HudBleep");
                }

                _lastClickTime = time;
                _lastSelectedName = selection.Name;
            } else {
                _loadColorsetButton.InputEnabled = false;
                _removeColorsetButton.InputEnabled = false;
                _lastSelectedName = "";
            }
        }

        private void OnGenerateColorSchemeClicked(object sender, EventArgs e) {
            GenerateColorSet();
        }

        private void OnLoadClicked(object sender, EventArgs e) {
            if (_colorsetList.Selection != null) {
                var selection = _colorsetList.Selection.AssocMember;
                Mod.Static.LoadColorSet(selection.Name);
                HudSoundUtils.PlaySound("HudBleep");
            }
        }

        private void OnMouseOver(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudMouseOver");
        }

        private void OnPresetChanged(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudMouseClick");
            GenerateColorSet(true);
        }

        private void OnRandomColorChanged(object sender, EventArgs e) {
            var checkbox = sender as BorderedCheckBox;
            if (checkbox != null) {
                _baseColorPicker.Visible = !checkbox.IsBoxChecked;
                _generateColorSchemeButton.Visible = checkbox.IsBoxChecked;
            }
        }

        private void OnRemoveClicked(object sender, EventArgs e) {
            if (_colorsetList.Selection != null) {
                var selection = _colorsetList.Selection.AssocMember;
                Mod.Static.RemoveColorSet(selection.Name);
                HudSoundUtils.PlaySound("HudLockingLost");
                LoadColorSets();
            }
        }

        private void OnSaveActiveColorsClicked(object sender, EventArgs e) {
            var window = new SaveWindow();
            window.Register(HudMain.HighDpiRoot);
            window.SaveClicked += (s, args) => {
                Mod.Static.SaveColorSet(window.Name);
                LoadColorSets();
            };
        }

        private void OnSaveGeneratedColorSchemeClicked(object sender, EventArgs e) {
            var colorSet = _schemePreview.ColorSet;
            var window = new SaveWindow(colorSet: colorSet);
            window.Register(HudMain.HighDpiRoot);
            window.SaveClicked += (s, args) => {
                var colorset = window.ColorSet;
                colorset.Name = window.Name;

                Mod.Static.SaveColorSet(colorset);
                LoadColorSets();
            };
        }

        private void OnSchemeChanged(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudMouseClick");
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