using RichHudFramework.UI;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.ColorSpace;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Globalization;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Builds a palette from a color scheme instead of fourteen separate picks.
    /// </summary>
    internal class GeneratorView : PanelView {
        /// <summary>
        /// The HSV picker is a fixed size, so the options column can never be narrower than it fits in.
        /// </summary>
        private const float MIN_OPTIONS_WIDTH = 318f;

        private const float MAX_OPTIONS_WIDTH = 500f;

        private readonly ColorPickerHSV2 _baseColorPicker;
        private readonly Dropdown<BaseSource> _baseSourceDropdown;
        private readonly ColorSchemeGenerator _generator = new ColorSchemeGenerator();
        private readonly Dropdown<int> _greyDropdown;
        private readonly bool[] _locked = new bool[ColorSet.SLOTS];
        private readonly SwatchGrid _paletteGrid;
        private readonly Dropdown<ColorSchemeGenerator.Preset> _presetDropdown;
        private readonly SwatchGrid _previewGrid;
        private readonly Dropdown<ColorSchemeGenerator.Scheme> _schemeDropdown;
        private readonly HudChain _sourceSection;
        private readonly SwatchGrid _sourceGrid;

        /// <summary>
        /// The scroll box entries of the two optional sections.
        /// </summary>
        private readonly ScrollBoxEntry<HudElementBase> _pickerEntry;

        private readonly ScrollBoxEntry<HudElementBase> _sourceEntry;

        private ColorSchemeGenerator.Result _result;

        /// <summary>
        /// Held across everything but the Generate button, so a changed option shows what it did.
        /// </summary>
        private int _seed;

        private ColorMask[] _sourceColors;
        private bool _suppressWrites;

        /// <summary>
        /// Where the color a scheme is grown from comes from.
        /// </summary>
        private enum BaseSource {
            Random,
            Palette,
            Target,
            Custom
        }

        public GeneratorView(float width, float height, HudParentBase parent = null) : base(width, height, parent) {
            var optionsWidth = MathHelper.Clamp(width * .45f, MIN_OPTIONS_WIDTH + LayoutMetrics.CARD_PADDING * 2f, MAX_OPTIONS_WIDTH);
            var previewWidth = width - optionsWidth - LayoutMetrics.SECTION_SPACING;

            var optionsCard = new Card(ModText.BC_UI_ColorSchemeGenerator.GetString(), optionsWidth, height);
            var optionsColumn = ControlFactory.CreateScrollColumn(Card.ContentWidth(optionsWidth), Card.ContentHeight(height));
            var optionsContentWidth = ControlFactory.ScrollContentWidth(Card.ContentWidth(optionsWidth));
            var controlWidth = ControlFactory.ControlWidth(optionsContentWidth);

            optionsCard.Content.Add(optionsColumn, 0f);

            _schemeDropdown = ControlFactory.CreateDropdown<ColorSchemeGenerator.Scheme>(controlWidth);
            _schemeDropdown.Add(ModText.BC_UI_Scheme_Default.GetString(), ColorSchemeGenerator.Scheme.Default);
            _schemeDropdown.Add(ModText.BC_UI_Scheme_Analogous.GetString(), ColorSchemeGenerator.Scheme.Analogous);
            _schemeDropdown.Add(ModText.BC_UI_Scheme_Complementary.GetString(), ColorSchemeGenerator.Scheme.Complementary);
            _schemeDropdown.Add(ModText.BC_UI_Scheme_SplitComplementary.GetString(), ColorSchemeGenerator.Scheme.SplitComplementary);
            _schemeDropdown.Add(ModText.BC_UI_Scheme_Triadic.GetString(), ColorSchemeGenerator.Scheme.Triadic);
            _schemeDropdown.Add(ModText.BC_UI_Scheme_Tetradic.GetString(), ColorSchemeGenerator.Scheme.Tetradic);
            _schemeDropdown.Add(ModText.BC_UI_Scheme_Square.GetString(), ColorSchemeGenerator.Scheme.Square);
            _schemeDropdown.Add(ModText.BC_UI_Scheme_Monochromatic.GetString(), ColorSchemeGenerator.Scheme.Monochromatic);
            _schemeDropdown.Add(ModText.BC_UI_Scheme_HullAndAccent.GetString(), ColorSchemeGenerator.Scheme.HullAndAccent);
            _schemeDropdown.SetSelection(ColorSchemeGenerator.Scheme.Default);

            _presetDropdown = ControlFactory.CreateDropdown<ColorSchemeGenerator.Preset>(controlWidth);
            _presetDropdown.Add(ModText.BC_UI_Preset_None.GetString(), ColorSchemeGenerator.Preset.None);
            _presetDropdown.Add(ModText.BC_UI_Preset_Pastel.GetString(), ColorSchemeGenerator.Preset.Pastel);
            _presetDropdown.Add(ModText.BC_UI_Preset_Soft.GetString(), ColorSchemeGenerator.Preset.Soft);
            _presetDropdown.Add(ModText.BC_UI_Preset_Light.GetString(), ColorSchemeGenerator.Preset.Light);
            _presetDropdown.Add(ModText.BC_UI_Preset_Hard.GetString(), ColorSchemeGenerator.Preset.Hard);
            _presetDropdown.Add(ModText.BC_UI_Preset_Pale.GetString(), ColorSchemeGenerator.Preset.Pale);
            _presetDropdown.Add(ModText.BC_UI_Preset_Vibrant.GetString(), ColorSchemeGenerator.Preset.Vibrant);
            _presetDropdown.Add(ModText.BC_UI_Preset_Muted.GetString(), ColorSchemeGenerator.Preset.Muted);
            _presetDropdown.Add(ModText.BC_UI_Preset_Warm.GetString(), ColorSchemeGenerator.Preset.Warm);
            _presetDropdown.Add(ModText.BC_UI_Preset_Cool.GetString(), ColorSchemeGenerator.Preset.Cool);
            _presetDropdown.Add(ModText.BC_UI_Preset_Dark.GetString(), ColorSchemeGenerator.Preset.Dark);
            _presetDropdown.Add(ModText.BC_UI_Preset_Lighter.GetString(), ColorSchemeGenerator.Preset.Lighter);
            _presetDropdown.SetSelection(ColorSchemeGenerator.Preset.None);

            _greyDropdown = ControlFactory.CreateDropdown<int>(controlWidth);
            for (var i = 0; i <= ColorSchemeGenerator.MAX_GREY_COUNT; i++) {
                _greyDropdown.Add(i.ToString(CultureInfo.CurrentCulture), i);
            }

            _greyDropdown.SetSelection(ColorSchemeGenerator.DEFAULT_GREY_COUNT);

            _baseSourceDropdown = ControlFactory.CreateDropdown<BaseSource>(controlWidth);
            _baseSourceDropdown.Add(ModText.BC_UI_BaseSource_Random.GetString(), BaseSource.Random);
            _baseSourceDropdown.Add(ModText.BC_UI_BaseSource_Palette.GetString(), BaseSource.Palette);
            _baseSourceDropdown.Add(ModText.BC_UI_BaseSource_Target.GetString(), BaseSource.Target);
            _baseSourceDropdown.Add(ModText.BC_UI_BaseSource_Custom.GetString(), BaseSource.Custom);
            _baseSourceDropdown.SetSelection(BaseSource.Random);

            _sourceGrid = new SwatchGrid(optionsContentWidth, SwatchGrid.MIN_ROW_HEIGHT + 8f, true);
            _sourceGrid.SlotClicked += OnSourceColorPicked;

            _sourceSection = ControlFactory.CreateColumn(optionsContentWidth, 0f, LayoutMetrics.ROW_SPACING);
            _sourceSection.Add(ControlFactory.CreateCaption(ModText.BC_UI_PickBaseHint.GetString(), optionsContentWidth), 0f);
            _sourceSection.Add(_sourceGrid, 0f);
            _sourceSection.Height = LayoutMetrics.LABEL_HEIGHT + _sourceGrid.Height + LayoutMetrics.ROW_SPACING;
            _sourceSection.Visible = false;

            _baseColorPicker = new ColorPickerHSV2() { Name = ModText.BC_UI_BaseColor.GetString() };

            var generateButton = ControlFactory.CreateButton(ModText.BC_UI_Generate.GetString(), role: ButtonRole.Primary);

            optionsColumn.Add(ControlFactory.CreateControlRow(ModText.BC_UI_ColorScheme.GetString(), _schemeDropdown, optionsContentWidth), 0f);
            optionsColumn.Add(ControlFactory.CreateControlRow(ModText.BC_UI_ColorPresets.GetString(), _presetDropdown, optionsContentWidth), 0f);
            optionsColumn.Add(ControlFactory.CreateControlRow(ModText.BC_UI_Greys.GetString(), _greyDropdown, optionsContentWidth), 0f);
            optionsColumn.Add(ControlFactory.CreateSeparator(optionsContentWidth), 0f);
            optionsColumn.Add(ControlFactory.CreateControlRow(ModText.BC_UI_BaseSource.GetString(), _baseSourceDropdown, optionsContentWidth), 0f);
            optionsColumn.Add(_sourceSection, 0f);
            _sourceEntry = optionsColumn[optionsColumn.Count - 1];

            optionsColumn.Add(_baseColorPicker, 0f);
            _pickerEntry = optionsColumn[optionsColumn.Count - 1];
            optionsColumn.Add(ControlFactory.CreateSeparator(optionsContentWidth), 0f);
            optionsColumn.Add(ControlFactory.CreateButtonRow(optionsContentWidth, generateButton), 0f);

            var previewCard = new Card(ModText.BC_UI_Preview.GetString(), previewWidth, height);
            var previewContentWidth = Card.ContentWidth(previewWidth);

            var gridSpace = Card.ContentHeight(height)
                - LayoutMetrics.LABEL_HEIGHT * 3f
                - LayoutMetrics.SEPARATOR_HEIGHT
                - LayoutMetrics.BUTTON_HEIGHT
                - LayoutMetrics.ROW_SPACING * 7f;

            var swatchRowHeight = SwatchGrid.FitRowHeight(gridSpace, 2);

            _previewGrid = new SwatchGrid(previewContentWidth, swatchRowHeight, true);
            _previewGrid.SlotClicked += OnSlotClicked;
            _previewGrid.SlotRightClicked += OnSlotRightClicked;

            _paletteGrid = new SwatchGrid(previewContentWidth, swatchRowHeight);

            var applyButton = ControlFactory.CreateButton(ModText.BC_UI_ApplyToPalette.GetString(), role: ButtonRole.Primary);
            var saveButton = ControlFactory.CreateButton(ModText.BC_UI_SaveAs.GetString());

            previewCard.Content.Add(ControlFactory.CreateCaption(ModText.BC_UI_GeneratedColors.GetString(), previewContentWidth), 0f);
            previewCard.Content.Add(_previewGrid, 0f);
            previewCard.Content.Add(ControlFactory.CreateCaption(ModText.BC_UI_SlotHint.GetString(), previewContentWidth), 0f);
            previewCard.Content.Add(ControlFactory.CreateSeparator(previewContentWidth), 0f);
            previewCard.Content.Add(ControlFactory.CreateCaption(ModText.BC_UI_CurrentPalette.GetString(), previewContentWidth), 0f);
            previewCard.Content.Add(_paletteGrid, 0f);
            previewCard.Content.Add(new EmptyHudElement() { Width = previewContentWidth }, 1f);
            previewCard.Content.Add(ControlFactory.CreateButtonRow(previewContentWidth, saveButton, applyButton), 0f);

            var layout = ControlFactory.CreateRow(width, height, LayoutMetrics.SECTION_SPACING);
            layout.Add(optionsCard, 0f);
            layout.Add(previewCard, 0f);

            layout.Register(this);

            _schemeDropdown.ValueChanged += OnOptionChanged;
            _presetDropdown.ValueChanged += OnOptionChanged;
            _greyDropdown.ValueChanged += OnOptionChanged;
            _baseSourceDropdown.ValueChanged += OnBaseSourceChanged;
            _baseColorPicker.ColorChanged += OnBaseColorChanged;

            generateButton.MouseInput.LeftClicked += OnGenerate;
            applyButton.MouseInput.LeftClicked += OnApplyToPalette;
            saveButton.MouseInput.LeftClicked += OnSave;

            _seed = _generator.NewSeed();

            UpdateBaseControls();
            Regenerate();
        }

        public override void Refresh() {
            if (Source == BaseSource.Target) {
                SampleTarget(false);
            }

            RefreshPalette();
        }

        private BaseSource Source {
            get { return _baseSourceDropdown.Value != null ? _baseSourceDropdown.Value.AssocMember : BaseSource.Random; }
        }

        private ColorSchemeGenerator.Options BuildOptions() {
            return new ColorSchemeGenerator.Options {
                Scheme = _schemeDropdown.Value != null ? _schemeDropdown.Value.AssocMember : ColorSchemeGenerator.Scheme.Default,
                Preset = _presetDropdown.Value != null ? _presetDropdown.Value.AssocMember : ColorSchemeGenerator.Preset.None,
                GreyCount = _greyDropdown.Value != null ? _greyDropdown.Value.AssocMember : ColorSchemeGenerator.DEFAULT_GREY_COUNT,
                BaseColor = Source == BaseSource.Random ? (HSL?)null : ReadPickerColor(),
                Seed = _seed,
                Locked = _locked,
                Current = _result.Masks,
            };
        }

        private void Regenerate() {
            _result = _generator.Generate(BuildOptions());

            if (Source == BaseSource.Random) {
                _suppressWrites = true;
                WritePickerColor(_result.BaseColor);
                _suppressWrites = false;
            }

            _previewGrid.SetMasks(_result.Masks);
            _previewGrid.SetLocked(_locked);
            RefreshPalette();
        }

        private HSL ReadPickerColor() {
            var color = _baseColorPicker.Color;

            return new HSV(color.X, color.Y / 100f, color.Z / 100f).ToHSL();
        }

        private void WritePickerColor(HSL color) {
            var hsv = color.ToHSV();

            _baseColorPicker.Color = new Vector3(hsv.H, hsv.S * 100f, hsv.V * 100f);
        }

        private void RefreshPalette() {
            _paletteGrid.SetMasks(Mod.Static?.GetCurrentPalette());
        }

        private void SetSource(BaseSource source) {
            _suppressWrites = true;
            _baseSourceDropdown.SetSelection(source);
            _suppressWrites = false;

            UpdateBaseControls();
        }

        /// <summary>
        /// Shows only the controls the chosen source needs.
        /// </summary>
        private void UpdateBaseControls() {
            var source = Source;

            var showPicker = source != BaseSource.Random;
            var showSource = (source == BaseSource.Palette || source == BaseSource.Target)
                && _sourceColors != null
                && _sourceColors.Length > 0;

            _baseColorPicker.Visible = showPicker;
            _pickerEntry.Enabled = showPicker;

            _sourceSection.Visible = showSource;
            _sourceEntry.Enabled = showSource;
        }

        private void OnGenerate(object sender, EventArgs args) {
            _seed = _generator.NewSeed();

            Regenerate();
        }

        private void OnOptionChanged(object sender, EventArgs args) {
            if (_suppressWrites) {
                return;
            }

            Regenerate();
        }

        /// <summary>
        /// Touching the picker means the color is being chosen by hand, whatever the source said before.
        /// </summary>
        private void OnBaseColorChanged(object sender, EventArgs args) {
            if (_suppressWrites) {
                return;
            }

            SetSource(BaseSource.Custom);
            ShowSourceColors(null);
            Regenerate();
        }

        private void OnBaseSourceChanged(object sender, EventArgs args) {
            if (_suppressWrites) {
                return;
            }

            switch (Source) {
                case BaseSource.Palette:
                    ShowSourceColors(Mod.Static?.GetCurrentPalette());
                    break;

                case BaseSource.Target:
                    SampleTarget(true);
                    break;

                default:
                    ShowSourceColors(null);
                    break;
            }

            Regenerate();
        }

        private void SampleTarget(bool report) {
            var colors = Mod.Static?.PaintJobService?.GetTargetedGridColors(ColorSet.SLOTS);

            if (colors == null || colors.Length == 0) {
                if (report) {
                    MyAPIGateway.Utilities.ShowMessage(Mod.NAME, ModText.BC_PaintJob_NoTarget.GetString());
                    HudSoundUtils.PlaySound("HudLockingLost");
                    SetSource(BaseSource.Custom);
                    ShowSourceColors(null);
                }

                return;
            }

            ShowSourceColors(colors);
        }

        /// <summary>
        /// Puts the colors that can be picked from on screen, starting on the most colorful.
        /// </summary>
        private void ShowSourceColors(ColorMask[] colors) {
            _sourceColors = colors;

            UpdateBaseControls();

            if (!_sourceEntry.Enabled) {
                return;
            }

            _sourceGrid.SetMasks(colors);
            SelectSourceColor(ColorSchemeGenerator.GetDominantIndex(colors));
        }

        private void OnSourceColorPicked(int index) {
            if (_sourceColors == null || index >= _sourceColors.Length) {
                return;
            }

            SelectSourceColor(index);
            Regenerate();
        }

        private void SelectSourceColor(int index) {
            _sourceGrid.SetSelected(index);

            _suppressWrites = true;
            WritePickerColor(ColorSchemeGenerator.ToHSL(_sourceColors[index]));
            _suppressWrites = false;
        }

        private void OnSlotClicked(int index) {
            if (ActiveDialog != null || _result.Masks == null || index >= _result.Masks.Length) {
                return;
            }

            var dialog = new ColorSlotDialog(index, _result.Masks[index]);

            dialog.Saved += (sender, args) => {
                _result.Masks[index] = dialog.Mask;

                _locked[index] = true;

                _previewGrid.SetMasks(_result.Masks);
                _previewGrid.SetLocked(_locked);
            };

            OpenDialog(dialog);
        }

        private void OnSlotRightClicked(int index) {
            if (index < 0 || index >= _locked.Length) {
                return;
            }

            _locked[index] = !_locked[index];
            _previewGrid.SetLocked(_locked);
        }

        private void OnApplyToPalette(object sender, EventArgs args) {
            if (_result.Masks == null) {
                return;
            }

            Mod.Static?.ApplySlots(_result.ToSlots());

            RefreshPalette();
            HudSoundUtils.PlaySound("HudBleep");
        }

        private void OnSave(object sender, EventArgs args) {
            if (_result.Masks == null) {
                return;
            }

            var masks = _result.Masks;
            var dialog = new SaveDialog(ModText.BC_UI_SaveAs.GetString(), ModText.BC_UI_GeneratedColorSetName.GetString());

            dialog.Saved += (sender2, args2) => {
                var name = dialog.Name;

                if (string.IsNullOrEmpty(name)) {
                    return;
                }

                Mod.Static?.SaveColorSet(new ColorSet(name, masks));
            };

            OpenDialog(dialog);
        }
    }
}
