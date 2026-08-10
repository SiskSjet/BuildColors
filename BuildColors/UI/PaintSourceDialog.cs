using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using VRageMath;

using static Sisk.BuildColors.UI.ControlFactory;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Editor for the paint source of a rule: where the color and skin of each matched block come from.
    /// </summary>
    public class PaintSourceDialog : DialogBase {
        private const float COLUMN_SPACING = 16f;

        /// <summary>
        /// Slack left at the bottom of every column so it cannot overlap itself.
        /// </summary>
        private const float COLUMN_SLACK = 12f;

        private const int MAX_ENTRIES = 32;
        private const int MIN_ENTRIES = 2;

        /// <summary>
        /// Floor derived from the tallest column, the entry editor.
        /// </summary>
        private const float MIN_DIALOG_HEIGHT = 600f;

        private const float MIN_DIALOG_WIDTH = 800f;
        private const float PREFERRED_DIALOG_HEIGHT = 700f;
        private const float PREFERRED_DIALOG_WIDTH = 1130f;

        /// <summary>
        /// Share of the content width taken by the settings and the list columns.
        /// </summary>
        private const float SETTINGS_COLUMN_SHARE = .34f;

        private const float LIST_COLUMN_SHARE = .27f;

        private readonly PaintColorSource _source;
        private readonly PaintSourceType _type;
        private readonly ColorModel _seedColor;
        private readonly List<SourceEntry> _entries = new List<SourceEntry>();

        private readonly Label _typeHintLabel;

        private readonly HudChain _axisRow;
        private readonly Dropdown<PaintSourceAxis> _axisDropdown;

        private readonly HudChain _blendRow;
        private readonly Dropdown<PaintBlendSpace> _blendDropdown;

        private readonly HudChain _fitRow;
        private readonly Dropdown<PaintGradientFit> _fitDropdown;
        private readonly Label _fitHint;

        private readonly HudChain _stepsRow;
        private readonly TextField _stepsField;
        private readonly Label _stepsHint;

        private readonly HudChain _scaleRow;
        private readonly TextField _scaleField;

        private readonly HudChain _periodRow;
        private readonly TextField _periodField;

        private readonly HudChain _shapeRow;
        private readonly Dropdown<PaintPatternShape> _shapeDropdown;

        private readonly HudChain _seedRow;
        private readonly TextField _seedField;

        private readonly HudChain _reverseRow;
        private readonly BorderedCheckBox _reverseCheckbox;

        private readonly Label _entriesLabel;
        private readonly ListBox<SourceEntry> _entryList;
        private readonly BorderedButton _addEntryButton;
        private readonly BorderedButton _removeEntryButton;

        private readonly ColorPickerHSV _entryColorPicker;
        private readonly ColorPaletteSelector _entryPalette;
        private readonly HudChain _positionRow;
        private readonly TextField _positionField;
        private readonly HudChain _weightRow;
        private readonly TextField _weightField;
        private readonly Dropdown<SkinListEntry, DefinitionCatalog.SkinOption> _entrySkinDropdown;

        private readonly Label _statusLabel;

        private SourceEntry _loadedEntry;

        public PaintSourceDialog(PaintColorSource source, ColorModel seedColor, HudParentBase parent = null) : base(parent) {
            _source = source;
            _seedColor = seedColor;

            _type = source.Type;

            var dialogWidth = MathHelper.Clamp(DialogSafeArea.GetAvailableWidth(), MIN_DIALOG_WIDTH, PREFERRED_DIALOG_WIDTH);
            var dialogHeight = GetSafeHeight(PREFERRED_DIALOG_HEIGHT, MIN_DIALOG_HEIGHT);

            Size = new Vector2(dialogWidth, dialogHeight);
            HeaderText = ModText.BC_UI_SourceEditorTitleFor.GetString(PaintSourceText.DescribeType(_type));

            var contentWidth = dialogWidth - Padding.X - LayoutMetrics.CONTENT_PADDING_X;
            var contentHeight = dialogHeight - Padding.Y - HEADER_HEIGHT - LayoutMetrics.CONTENT_PADDING_Y;

            var columnHeight = contentHeight
                - LayoutMetrics.STATUS_HEIGHT
                - LayoutMetrics.BUTTON_HEIGHT
                - LayoutMetrics.SECTION_SPACING * 2f;

            var settingsWidth = (float)Math.Floor(contentWidth * SETTINGS_COLUMN_SHARE);
            var listWidth = (float)Math.Floor(contentWidth * LIST_COLUMN_SHARE);
            var entryWidth = contentWidth - settingsWidth - listWidth - COLUMN_SPACING * 2f;

            _typeHintLabel = CreateLabel(PaintSourceText.DescribeHint(_type), settingsWidth);

            _axisDropdown = CreateDropdown<PaintSourceAxis>(ControlWidth(settingsWidth));
            _axisDropdown.Add(ModText.BC_UI_Axis_Longest.GetString(), PaintSourceAxis.Longest);
            _axisDropdown.Add(ModText.BC_UI_Axis_GridX.GetString(), PaintSourceAxis.GridX);
            _axisDropdown.Add(ModText.BC_UI_Axis_GridY.GetString(), PaintSourceAxis.GridY);
            _axisDropdown.Add(ModText.BC_UI_Axis_GridZ.GetString(), PaintSourceAxis.GridZ);
            _axisDropdown.Add(ModText.BC_UI_Axis_WorldUp.GetString(), PaintSourceAxis.WorldUp);
            _axisDropdown.Add(ModText.BC_UI_Axis_Radial.GetString(), PaintSourceAxis.Radial);
            _axisRow = CreateControlRow(ModText.BC_UI_Axis.GetString(), _axisDropdown, settingsWidth);

            _blendDropdown = CreateDropdown<PaintBlendSpace>(ControlWidth(settingsWidth));
            _blendDropdown.Add(ModText.BC_UI_Blend_Lab.GetString(), PaintBlendSpace.Lab);
            _blendDropdown.Add(ModText.BC_UI_Blend_Hsv.GetString(), PaintBlendSpace.Hsv);
            _blendDropdown.Add(ModText.BC_UI_Blend_Rgb.GetString(), PaintBlendSpace.Rgb);
            _blendRow = CreateControlRow(ModText.BC_UI_BlendSpace.GetString(), _blendDropdown, settingsWidth);

            _fitDropdown = CreateDropdown<PaintGradientFit>(ControlWidth(settingsWidth));
            _fitDropdown.Add(ModText.BC_UI_Fit_MatchedBlocks.GetString(), PaintGradientFit.MatchedBlocks);
            _fitDropdown.Add(ModText.BC_UI_Fit_GridBounds.GetString(), PaintGradientFit.GridBounds);
            _fitRow = CreateControlRow(ModText.BC_UI_Fit.GetString(), _fitDropdown, settingsWidth);
            _fitHint = CreateLabel(ModText.BC_UI_SourceHint_Fit.GetString(), settingsWidth);

            _stepsField = CreateTextField(ControlWidth(settingsWidth));
            _stepsRow = CreateControlRow(ModText.BC_UI_Steps.GetString(), _stepsField, settingsWidth);
            _stepsHint = CreateLabel(ModText.BC_UI_SourceHint_Steps.GetString(), settingsWidth);

            _scaleField = CreateTextField(ControlWidth(settingsWidth));
            _scaleRow = CreateControlRow(ModText.BC_UI_Scale.GetString(), _scaleField, settingsWidth);

            _periodField = CreateTextField(ControlWidth(settingsWidth));
            _periodRow = CreateControlRow(ModText.BC_UI_Period.GetString(), _periodField, settingsWidth);

            _shapeDropdown = CreateDropdown<PaintPatternShape>(ControlWidth(settingsWidth));
            _shapeDropdown.Add(ModText.BC_UI_Shape_Stripes.GetString(), PaintPatternShape.Stripes);
            _shapeDropdown.Add(ModText.BC_UI_Shape_Checker.GetString(), PaintPatternShape.Checker);
            _shapeRow = CreateControlRow(ModText.BC_UI_Shape.GetString(), _shapeDropdown, settingsWidth);

            _seedField = CreateTextField(ControlWidth(settingsWidth));
            _seedRow = CreateControlRow(ModText.BC_UI_Seed.GetString(), _seedField, settingsWidth);

            _reverseCheckbox = CreateCheckbox();
            _reverseRow = CreateCheckboxRow(_reverseCheckbox, ModText.BC_UI_Reverse.GetString(), settingsWidth);

            var settingsColumn = CreateColumn(settingsWidth, columnHeight);
            settingsColumn.Add(_typeHintLabel, 0f);
            settingsColumn.Add(CreateSeparator(settingsWidth), 0f);
            settingsColumn.Add(_axisRow, 0f);
            settingsColumn.Add(_shapeRow, 0f);
            settingsColumn.Add(_periodRow, 0f);
            settingsColumn.Add(_scaleRow, 0f);
            settingsColumn.Add(_blendRow, 0f);
            settingsColumn.Add(_stepsRow, 0f);
            settingsColumn.Add(_stepsHint, 0f);
            settingsColumn.Add(_fitRow, 0f);
            settingsColumn.Add(_fitHint, 0f);
            settingsColumn.Add(_seedRow, 0f);
            settingsColumn.Add(_reverseRow, 0f);

            _entriesLabel = CreateLabel(string.Empty, listWidth);

            _addEntryButton = CreateButton(ModText.BC_UI_AddEntry.GetString());
            _removeEntryButton = CreateButton(ModText.BC_UI_RemoveEntry.GetString());

            var listHeight = columnHeight
                - LayoutMetrics.LABEL_HEIGHT
                - LayoutMetrics.BUTTON_HEIGHT
                - LayoutMetrics.SECTION_SPACING * 2f
                - COLUMN_SLACK;

            _entryList = CreateList<SourceEntry>(listWidth, listHeight);

            var listColumn = CreateColumn(listWidth, columnHeight);
            listColumn.Add(_entriesLabel, 0f);
            listColumn.Add(_entryList, 0f);
            listColumn.Add(CreateButtonRow(listWidth, _addEntryButton, _removeEntryButton), 0f);

            _entryColorPicker = new ColorPickerHSV() {
                Width = entryWidth,
                Height = LayoutMetrics.COLOR_PICKER_HEIGHT,
                Name = ModText.BC_UI_EntryColor.GetString(),
            };

            _entryPalette = new ColorPaletteSelector() { Width = entryWidth };
            _entryPalette.ColorPicked += OnPaletteColorPicked;

            _positionField = CreateTextField(ControlWidth(entryWidth));
            _positionRow = CreateControlRow(ModText.BC_UI_StopPosition.GetString(), _positionField, entryWidth);

            _weightField = CreateTextField(ControlWidth(entryWidth));
            _weightRow = CreateControlRow(ModText.BC_UI_EntryWeight.GetString(), _weightField, entryWidth);

            _entrySkinDropdown = DefinitionCatalog.CreateSkinDropdown(LayoutMetrics.CONTROL_HEIGHT);
            _entrySkinDropdown.DimAlignment = DimAlignments.None;
            _entrySkinDropdown.Width = entryWidth;

            var entryColumn = CreateColumn(entryWidth, columnHeight);
            entryColumn.Add(CreateLabel(ModText.BC_UI_SelectedEntry.GetString(), entryWidth), 0f);
            entryColumn.Add(_entryColorPicker, 0f);
            entryColumn.Add(_entryPalette, 0f);
            entryColumn.Add(_positionRow, 0f);
            entryColumn.Add(_weightRow, 0f);
            entryColumn.Add(CreateLabel(ModText.BC_UI_TargetSkin.GetString(), entryWidth), 0f);
            entryColumn.Add(_entrySkinDropdown, 0f);

            var columns = new HudChain(false) {
                CollectionContainer = { settingsColumn, listColumn, entryColumn },
                Spacing = COLUMN_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = columnHeight,
            };

            _statusLabel = CreateCaption(string.Empty, contentWidth, LayoutMetrics.STATUS_HEIGHT);

            var cancelButton = CreateButton(ModText.BC_UI_Cancel.GetString(), 150f);
            var saveButton = CreateButton(ModText.BC_UI_Save.GetString(), 150f, ButtonRole.Primary);

            var buttonRow = new HudChain(false) {
                CollectionContainer = { cancelButton, saveButton },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.AlignMembersEnd,
                Width = contentWidth,
                Height = LayoutMetrics.BUTTON_HEIGHT,
            };

            var layout = new HudChain(true, body) {
                ParentAlignment = ParentAlignments.Inner,
                DimAlignment = DimAlignments.UnpaddedSize,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                CollectionContainer = { columns, _statusLabel, buttonRow },
                Spacing = LayoutMetrics.SECTION_SPACING,
                Padding = new Vector2(LayoutMetrics.CONTENT_PADDING_X, LayoutMetrics.CONTENT_PADDING_Y)
            };

            _entryList.ValueChanged += OnEntrySelectionChanged;
            _addEntryButton.MouseInput.LeftClicked += OnAddEntry;
            _removeEntryButton.MouseInput.LeftClicked += OnRemoveEntry;

            saveButton.MouseInput.LeftClicked += OnSaveClicked;
            cancelButton.MouseInput.LeftClicked += OnCancelClicked;

            LoadSource();
        }

        public event RichHudFramework.EventHandler Saved;

        public bool WasSaved { get; private set; }

        private static string DescribeEntry(int index, SourceEntry entry, PaintSourceType type) {
            var color = string.Format("#{0:X2}{1:X2}{2:X2}", entry.Color.R, entry.Color.G, entry.Color.B);
            var detail = string.Empty;

            if (type == PaintSourceType.Gradient) {
                detail = string.Format(" @{0}", entry.Position.ToString("0.##", CultureInfo.CurrentCulture));
            } else if (type == PaintSourceType.Scatter) {
                detail = string.Format(" x{0}", entry.Weight.ToString("0.##", CultureInfo.CurrentCulture));
            }

            return string.Format("{0}. {1}{2}", index + 1, color, detail);
        }

        private static bool TryParseNumber(string text, out float value) {
            return float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out value)
                || float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseCount(string text, out int value) {
            return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out value)
                || int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private void LoadSource() {
            _axisDropdown.SetSelection(_source.Axis);
            _blendDropdown.SetSelection(_source.Blend);
            _shapeDropdown.SetSelection(_source.Shape);
            _fitDropdown.SetSelection(_source.Fit);

            _stepsField.Text = _source.Steps.ToString(CultureInfo.CurrentCulture);
            _scaleField.Text = _source.Scale.ToString("0.##", CultureInfo.CurrentCulture);
            _periodField.Text = _source.Period.ToString(CultureInfo.CurrentCulture);
            _seedField.Text = _source.Seed.ToString(CultureInfo.CurrentCulture);
            _reverseCheckbox.Value = _source.Reverse;

            _entries.Clear();

            if (_source.UsesStops) {
                if (_source.Stops != null) {
                    foreach (var stop in _source.Stops) {
                        _entries.Add(new SourceEntry { Color = stop.Color, Position = stop.Position, SkinId = stop.SkinId, Weight = 1f });
                    }
                }
            } else if (_source.Palette != null) {
                foreach (var entry in _source.Palette) {
                    _entries.Add(new SourceEntry { Color = entry.Color, Position = 0f, SkinId = entry.SkinId, Weight = entry.Weight });
                }
            }

            EnsureEntries();
            RefreshEntries();
            UpdateVisibility();
        }

        /// <summary>
        /// Makes sure there is something to edit.
        /// </summary>
        private void EnsureEntries() {
            while (_entries.Count < MIN_ENTRIES) {
                _entries.Add(new SourceEntry {
                    Color = _seedColor,
                    Position = _entries.Count,
                    SkinId = string.Empty,
                    Weight = 1f
                });
            }
        }

        /// <summary>
        /// Only the controls the chosen kind of source reads are on screen.
        /// </summary>
        private void UpdateVisibility() {
            var isGradient = _type == PaintSourceType.Gradient;
            var isCamo = _type == PaintSourceType.Camo;
            var isScatter = _type == PaintSourceType.Scatter;
            var isPattern = _type == PaintSourceType.Pattern;

            _axisRow.Visible = isGradient || isPattern;
            _blendRow.Visible = isGradient;
            _stepsRow.Visible = isGradient;
            _stepsHint.Visible = isGradient;
            _fitRow.Visible = isGradient;
            _fitHint.Visible = isGradient;
            _reverseRow.Visible = isGradient;
            _scaleRow.Visible = isCamo;
            _periodRow.Visible = isPattern;
            _shapeRow.Visible = isPattern;
            _seedRow.Visible = isCamo || isScatter;

            _positionRow.Visible = isGradient;
            _weightRow.Visible = isScatter;

            _entriesLabel.Text = isGradient ? ModText.BC_UI_Stops.GetString() : ModText.BC_UI_Palette.GetString();

            UpdateEntryButtonState();
        }

        private void RefreshEntries(SourceEntry entryToSelect = null) {
            _entryList.ClearEntries();

            for (var i = 0; i < _entries.Count; i++) {
                _entryList.Add(DescribeEntry(i, _entries[i], _type), _entries[i]);
            }

            if (_entryList.Count == 0) {
                _loadedEntry = null;
                return;
            }

            var index = entryToSelect != null ? _entries.IndexOf(entryToSelect) : 0;
            _entryList.SetSelectionAt(index >= 0 && index < _entryList.Count ? index : 0);
        }

        private void SyncEntryLabel(SourceEntry entry) {
            if (entry == null) {
                return;
            }

            var index = _entries.IndexOf(entry);
            if (index < 0 || index >= _entryList.EntryList.Count) {
                return;
            }

            _entryList.EntryList[index].Element.TextBoard.SetText(DescribeEntry(index, entry, _type));
        }

        private SourceEntry GetSelectedEntry() {
            return _entryList.Value != null ? _entryList.Value.AssocMember : null;
        }

        private void OnEntrySelectionChanged(object sender, EventArgs e) {
            StoreEntry(_loadedEntry);
            SyncEntryLabel(_loadedEntry);

            var entry = GetSelectedEntry();
            _loadedEntry = entry;

            if (entry == null) {
                return;
            }

            _entryColorPicker.Value = new VRageMath.Color(entry.Color.R, entry.Color.G, entry.Color.B);
            _positionField.Text = entry.Position.ToString("0.##", CultureInfo.CurrentCulture);
            _weightField.Text = entry.Weight.ToString("0.##", CultureInfo.CurrentCulture);

            var skinIndex = DefinitionCatalog.IndexOfSkin(entry.SkinId);
            _entrySkinDropdown.SetSelectionAt(skinIndex >= 0 ? skinIndex : 0);

            UpdateEntryButtonState();
        }

        /// <summary>
        /// Writes the editor back into the entry it was loaded from.
        /// </summary>
        private void StoreEntry(SourceEntry entry) {
            if (entry == null || !_entries.Contains(entry)) {
                return;
            }

            var color = _entryColorPicker.Value;
            entry.Color = new ColorModel(color.R, color.G, color.B);

            float position;
            if (TryParseNumber(_positionField.Text.ToString(), out position)) {
                entry.Position = MathHelper.Clamp(position, 0f, 1f);
            }

            float weight;
            if (TryParseNumber(_weightField.Text.ToString(), out weight) && weight >= 0f) {
                entry.Weight = weight;
            }

            var skin = _entrySkinDropdown.Value != null ? _entrySkinDropdown.Value.AssocMember : null;
            entry.SkinId = skin != null ? skin.SkinId : string.Empty;
        }

        private void OnAddEntry(object sender, EventArgs e) {
            if (_entries.Count >= MAX_ENTRIES) {
                _statusLabel.Text = ModText.BC_UI_Status_EntryLimit.GetString(MAX_ENTRIES);
                HudSoundUtils.PlaySound("HudLockingLost");
                return;
            }

            StoreEntry(_loadedEntry);

            var previous = _entries.Count > 0 ? _entries[_entries.Count - 1] : null;
            var entry = new SourceEntry {
                Color = previous != null ? previous.Color : _seedColor,
                Position = 1f,
                SkinId = previous != null ? previous.SkinId : string.Empty,
                Weight = 1f
            };

            _entries.Add(entry);
            RefreshEntries(entry);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnRemoveEntry(object sender, EventArgs e) {
            var entry = GetSelectedEntry();
            if (entry == null) {
                return;
            }

            if (_entries.Count <= MIN_ENTRIES) {
                _statusLabel.Text = ModText.BC_UI_Status_EntriesRequired.GetString(MIN_ENTRIES);
                HudSoundUtils.PlaySound("HudLockingLost");
                return;
            }

            _entries.Remove(entry);
            _loadedEntry = null;

            RefreshEntries();
            HudSoundUtils.PlaySound("HudLockingLost");
        }

        private void UpdateEntryButtonState() {
            _removeEntryButton.InputEnabled = _entries.Count > MIN_ENTRIES && GetSelectedEntry() != null;
            _addEntryButton.InputEnabled = _entries.Count < MAX_ENTRIES;
        }

        private void OnPaletteColorPicked(VRageMath.Color color) {
            _entryColorPicker.Value = color;
        }

        private void OnSaveClicked(object sender, EventArgs e) {
            if (!ApplyChanges()) {
                HudSoundUtils.PlaySound("HudLockingLost");
                return;
            }

            WasSaved = true;
            HudSoundUtils.PlaySound("HudBleep");
            Saved?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void OnCancelClicked(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudLockingLost");
            Close();
        }

        private bool ApplyChanges() {
            StoreEntry(_loadedEntry);

            var steps = _source.Steps;
            var scale = _source.Scale;
            var period = _source.Period;
            var seed = _source.Seed;

            int parsedSteps;
            var stepsRead = TryParseCount(_stepsField.Text.ToString(), out parsedSteps);

            if (_type == PaintSourceType.Gradient && (!stepsRead || parsedSteps < 0 || parsedSteps > PaintColorSource.MAX_STEPS)) {
                _statusLabel.Text = ModText.BC_UI_Status_StepsRange.GetString(PaintColorSource.MAX_STEPS);
                return false;
            }

            float parsedScale;
            var scaleRead = TryParseNumber(_scaleField.Text.ToString(), out parsedScale);

            if (_type == PaintSourceType.Camo
                && (!scaleRead || parsedScale < PaintColorSource.MIN_SCALE || parsedScale > PaintColorSource.MAX_SCALE)) {
                _statusLabel.Text = ModText.BC_UI_Status_ScaleRange.GetString(PaintColorSource.MIN_SCALE, PaintColorSource.MAX_SCALE);
                return false;
            }

            int parsedPeriod;
            var periodRead = TryParseCount(_periodField.Text.ToString(), out parsedPeriod);

            if (_type == PaintSourceType.Pattern && (!periodRead || parsedPeriod < 1)) {
                _statusLabel.Text = ModText.BC_UI_Status_PeriodRange.GetString();
                return false;
            }

            int parsedSeed;
            if (TryParseCount(_seedField.Text.ToString(), out parsedSeed)) {
                seed = parsedSeed;
            }

            if (stepsRead) {
                steps = parsedSteps;
            }

            if (scaleRead) {
                scale = parsedScale;
            }

            if (periodRead) {
                period = parsedPeriod;
            }

            _source.Axis = _axisDropdown.Value != null ? _axisDropdown.Value.AssocMember : _source.Axis;
            _source.Blend = _blendDropdown.Value != null ? _blendDropdown.Value.AssocMember : _source.Blend;
            _source.Shape = _shapeDropdown.Value != null ? _shapeDropdown.Value.AssocMember : _source.Shape;
            _source.Fit = _fitDropdown.Value != null ? _fitDropdown.Value.AssocMember : _source.Fit;
            _source.Steps = MathHelper.Clamp(steps, 0, PaintColorSource.MAX_STEPS);
            _source.Scale = MathHelper.Clamp(scale, PaintColorSource.MIN_SCALE, PaintColorSource.MAX_SCALE);
            _source.Period = period > 0 ? period : 1;
            _source.Seed = seed;
            _source.Reverse = _reverseCheckbox.Value;

            if (_source.UsesStops) {
                _source.Stops = new List<PaintColorStop>();

                foreach (var entry in _entries) {
                    _source.Stops.Add(new PaintColorStop(entry.Color, entry.Position) { SkinId = entry.SkinId });
                }
            } else {
                _source.Palette = new List<PaintPaletteEntry>();

                foreach (var entry in _entries) {
                    _source.Palette.Add(new PaintPaletteEntry(entry.Color) { SkinId = entry.SkinId, Weight = entry.Weight });
                }
            }

            _statusLabel.Text = string.Empty;

            return true;
        }

        /// <summary>
        /// One color as the dialog edits it.
        /// </summary>
        private sealed class SourceEntry {
            public ColorModel Color;
            public float Position;
            public string SkinId;
            public float Weight;
        }
    }
}
