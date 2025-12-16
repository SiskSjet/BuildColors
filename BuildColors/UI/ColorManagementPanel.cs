using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.ColorSpace;
using System;
using System.Linq;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Right-side panel for color set management and color scheme generation.
    /// Styled like a window but implemented as a panel component.
    /// </summary>
    public class ColorManagementPanel : HudElementBase {
        public const float ASPECT_RATIO_END = 5f / 4f;
        public const float ASPECT_RATIO_START = 16f / 9f;
        public const float OFFSET_END = -60;
        public const float OFFSET_START = 200;
        public const float SLOPE = (OFFSET_END - OFFSET_START) / (ASPECT_RATIO_END - ASPECT_RATIO_START);
        public const float WIDTH = 500f;
        public const float Y_INTERCEPT = OFFSET_START - SLOPE * ASPECT_RATIO_START;
        private const float HEIGHT = 1080f;

        private readonly ColorPickerHSV2 _baseColorPicker;
        private readonly TexturedBox _border;
        private readonly TexturedBox _body;
        private readonly ListBox<ColorSet> _colorsetList;
        private readonly ColorSetElement _colorsetPreview;
        private readonly BorderedButton _generateColorSchemeButton;
        private readonly Label _header;
        private readonly BorderedButton _loadColorsetButton;
        private readonly Dropdown<ColorSchemeGenerator.Preset> _presetDropdown;
        private readonly BorderedButton _removeColorsetButton;
        private readonly Dropdown<ColorSchemeGenerator.Scheme> _schemeDropdown;
        private readonly ColorSchemeGenerator _schemeGenerator;
        private readonly ColorSetElement _schemePreview;
        private DateTime _lastClickTime;
        private string _lastSelectedName;

        public ColorManagementPanel(HudParentBase parent = null) : base(parent) {
            Size = new Vector2(WIDTH, HEIGHT);

            // Window-style border
            _border = new TexturedBox(this) {
                DimAlignment = DimAlignments.Both,
                Color = Style.BorderColor,
            };

            // Window-style body background
            _body = new TexturedBox(this) {
                DimAlignment = DimAlignments.Both,
                Color = Style.BodyBackgroundColor,
                Padding = new Vector2(2f),
            };

            // Header
            var headerBackground = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = 20,
                Color = VRageMath.Color.Transparent,
            };

            _header = new Label() {
                Text = Mod.NAME,
                Format = Style.HeaderText,
                Padding = new Vector2(50f, 0f),
            };

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

            var colorsetPreviewSeperator = new TexturedBox() {
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
                Height = 250f,
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
                Value = true,
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

            var headerLayout = new HudChain(true) {
                CollectionContainer = { headerBackground, _header, headerSeperator},
                Spacing = 10f,
            };

            var colorsetLayout = new HudChain(true) {
                CollectionContainer = { colorsetLabel, _colorsetList, colorsetListSeperator, _colorsetPreview, colorsetPreviewSeperator, colorsetControls, colorsetSeperator },
                Spacing = 10f,
            };

            var generatorLayout = new HudChain(true) {
                CollectionContainer = { colorSchemeGeneratorLabel, schemePresetLayout, randomColorLayout, _baseColorPicker, generatorOptionsSeperator, generatorControls, generatorControlsSeperator, _schemePreview },
                Spacing = 10f,
            };

            var mainLayout = new HudChain(true, _body) {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV,
                CollectionContainer = { headerLayout, colorsetLayout, generatorLayout },
                Spacing = 10f,
                Padding = new Vector2(10f, 0f),
            };

            _schemeGenerator = new ColorSchemeGenerator();

            // Wire up events
            randomColorCheckbox.MouseInput.LeftClicked += OnRandomColorChanged;
            _baseColorPicker.ColorChanged += OnBaseColorChanged;

            _colorsetList.ValueChanged += OnColorSetChanged;
            _colorsetList.MouseInput.CursorEntered += OnMouseOver;

            _loadColorsetButton.MouseInput.LeftClicked += OnLoadClicked;
            _loadColorsetButton.MouseInput.CursorEntered += OnMouseOver;

            _removeColorsetButton.MouseInput.LeftClicked += OnRemoveClicked;
            _removeColorsetButton.MouseInput.CursorEntered += OnMouseOver;

            saveActivColorsButton.MouseInput.LeftClicked += OnSaveActiveColorsClicked;
            saveActivColorsButton.MouseInput.CursorEntered += OnMouseOver;

            _schemeDropdown.ValueChanged += OnSchemeChanged;
            _schemeDropdown.MouseInput.CursorEntered += OnMouseOver;

            _presetDropdown.ValueChanged += OnPresetChanged;
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

        public event RichHudFramework.EventHandler DialogRequested;

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

            if (MyAPIGateway.Session?.Camera == null) {
                return;
            }

            var screenWidth = MyAPIGateway.Session.Camera.ViewportSize.X;
            var screenHeight = MyAPIGateway.Session.Camera.ViewportSize.Y;
            var aspectRatio = screenWidth / screenHeight;

            var offset = SLOPE * aspectRatio + Y_INTERCEPT;
            Offset = new Vector2(offset, 0f);
        }

        private void GenerateColorSet(bool reuseColor = false) {
            var generator = _schemeGenerator;

            var preset = _presetDropdown.Value.AssocMember;
            var scheme = _schemeDropdown.Value.AssocMember;

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
            if (_colorsetList.Value != null) {
                var selection = _colorsetList.Value.AssocMember;
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
            if (_colorsetList.Value != null) {
                var selection = _colorsetList.Value.AssocMember;
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
                _baseColorPicker.Visible = !checkbox.Value;
                _generateColorSchemeButton.Visible = checkbox.Value;
            }
        }

        private void OnRemoveClicked(object sender, EventArgs e) {
            if (_colorsetList.Value != null) {
                var selection = _colorsetList.Value.AssocMember;
                Mod.Static.RemoveColorSet(selection.Name);
                HudSoundUtils.PlaySound("HudLockingLost");
                LoadColorSets();
            }
        }

        private void OnSaveActiveColorsClicked(object sender, EventArgs e) {
            var dialog = new SaveDialog();
            dialog.SaveClicked += (s, args) => {
                Mod.Static.SaveColorSet(dialog.Name);
                LoadColorSets();
            };
            OnDialogRequested(dialog);
        }

        private void OnSaveGeneratedColorSchemeClicked(object sender, EventArgs e) {
            var colorSet = _schemePreview.ColorSet;
            var dialog = new SaveDialog(colorSet: colorSet);
            dialog.SaveClicked += (s, args) => {
                var colorset = dialog.ColorSet;
                colorset.Name = dialog.Name;

                Mod.Static.SaveColorSet(colorset);
                LoadColorSets();
            };
            OnDialogRequested(dialog);
        }

        private void OnSchemeChanged(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudMouseClick");
            GenerateColorSet(true);
        }

        private void SetOpacity() {
            var opacity = MyAPIGateway.Session?.Config?.UIBkOpacity ?? 1f;

            _border.Color = _border.Color.SetAlphaPct(opacity);
            _body.Color = _body.Color.SetAlphaPct(opacity);
        }

        private void OnDialogRequested(DialogBase dialog) {
            DialogRequested?.Invoke(this, new DialogRequestedEventArgs { Dialog = dialog });
        }
    }
}
