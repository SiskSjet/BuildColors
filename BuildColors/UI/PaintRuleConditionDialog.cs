using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Globalization;
using VRageMath;

using static Sisk.BuildColors.UI.ControlFactory;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Modal dialog for configuring a single paint rule condition.
    /// </summary>
    public class PaintRuleConditionDialog : DialogBase {
        private const float BUTTON_ROW_HEIGHT = LayoutMetrics.CONTROL_HEIGHT;
        private const float CONTROL_HEIGHT = LayoutMetrics.CONTROL_HEIGHT;
        private const float MIN_DIALOG_HEIGHT = 660f;
        private const float PREFERRED_DIALOG_HEIGHT = 700f;

        /// <summary>
        /// Preferred width.
        /// </summary>
        private const float PREFERRED_DIALOG_WIDTH = 620f;

        /// <summary>
        /// Narrowest width that still leaves the color picker its minimum usable width.
        /// </summary>
        private const float MIN_DIALOG_WIDTH = 480f;
        private const float LABEL_HEIGHT = LayoutMetrics.LABEL_HEIGHT;
        private const float ROW_SPACING = LayoutMetrics.ROW_SPACING;
        private const float SECTION_LABEL_WIDTH = 120f;
        private const float STATUS_HEIGHT = LayoutMetrics.STATUS_HEIGHT;

        private readonly PaintRuleCondition _condition;
        private readonly Dropdown<PaintRuleConditionType> _conditionTypeDropdown;
        private readonly Dropdown<PaintRuleComparison> _comparisonDropdown;
        private readonly Label _comparisonLabel;

        private readonly HudChain _colorSection;
        private readonly ColorPickerHSV2 _colorPicker;
        private readonly ColorPaletteSelector _palette;

        private readonly HudChain _definitionSection;
        private readonly Dropdown<DefinitionCatalog.BlockDefinitionOption> _definitionDropdown;
        private readonly TextField _definitionTypeField;
        private readonly TextField _definitionSubtypeField;

        private readonly HudChain _skinSection;
        private readonly Dropdown<SkinListEntry, DefinitionCatalog.SkinOption> _skinDropdown;

        private readonly HudChain _categorySection;
        private readonly Dropdown<PaintRuleBlockCategory> _categoryDropdown;

        private readonly HudChain _gridSizeSection;
        private readonly Dropdown<PaintRuleGridSize> _gridSizeDropdown;

        private readonly HudChain _integritySection;
        private readonly Dropdown<PaintRuleIntegrityState> _integrityDropdown;
        private readonly TextField _integrityThresholdField;
        private readonly HudChain _integrityThresholdRow;
        private readonly Label _integrityThresholdHint;

        private readonly HudChain _anyBlockSection;

        private readonly Label _statusLabel;

        public PaintRuleConditionDialog(PaintRuleCondition condition, HudParentBase parent = null) : base(parent) {
            _condition = condition ?? new PaintRuleCondition();

            var dialogWidth = MathHelper.Clamp(DialogSafeArea.GetAvailableWidth(), MIN_DIALOG_WIDTH, PREFERRED_DIALOG_WIDTH);

            var dialogHeight = GetSafeHeight(PREFERRED_DIALOG_HEIGHT, MIN_DIALOG_HEIGHT);

            Size = new Vector2(dialogWidth, dialogHeight);
            HeaderText = ModText.BC_UI_ConditionEditorTitle.GetString();

            var contentWidth = ContentWidth(dialogWidth);

            var typeLabel = CreateFullWidthLabel(ModText.BC_UI_ConditionType.GetString());
            _conditionTypeDropdown = CreateFullWidthDropdown<PaintRuleConditionType>();
            _conditionTypeDropdown.Add(ModText.BC_UI_ConditionType_BlockColor.GetString(), PaintRuleConditionType.BlockColor);
            _conditionTypeDropdown.Add(ModText.BC_UI_ConditionType_BlockDefinition.GetString(), PaintRuleConditionType.BlockDefinition);
            _conditionTypeDropdown.Add(ModText.BC_UI_ConditionType_BlockSkin.GetString(), PaintRuleConditionType.BlockSkin);
            _conditionTypeDropdown.Add(ModText.BC_UI_ConditionType_BlockCategory.GetString(), PaintRuleConditionType.BlockCategory);
            _conditionTypeDropdown.Add(ModText.BC_UI_ConditionType_GridSize.GetString(), PaintRuleConditionType.GridSize);
            _conditionTypeDropdown.Add(ModText.BC_UI_ConditionType_BlockIntegrity.GetString(), PaintRuleConditionType.BlockIntegrity);
            _conditionTypeDropdown.Add(ModText.BC_UI_ConditionType_AnyBlock.GetString(), PaintRuleConditionType.AnyBlock);

            _comparisonLabel = CreateFullWidthLabel(ModText.BC_UI_Comparison.GetString());
            _comparisonDropdown = CreateFullWidthDropdown<PaintRuleComparison>();
            _comparisonDropdown.Add(ModText.BC_UI_Comparison_Matches.GetString(), PaintRuleComparison.Equals);
            _comparisonDropdown.Add(ModText.BC_UI_Comparison_DoesNotMatch.GetString(), PaintRuleComparison.NotEquals);

            _colorPicker = new ColorPickerHSV2() {
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.COLOR_PICKER_HEIGHT,
                Name = ModText.BC_UI_ConditionType_BlockColor.GetString(),
            };

            _palette = new ColorPaletteSelector() { DimAlignment = DimAlignments.Width };
            _palette.ColorPicked += OnPaletteColorPicked;

            _colorSection = new HudChain(true) {
                CollectionContainer = {
                    CreateFullWidthCaption(ModText.BC_UI_ConditionHint_BlockColor.GetString()),
                    _colorPicker,
                    CreateFullWidthCaption(ModText.BC_UI_PickFromPalette.GetString()),
                    _palette
                },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LABEL_HEIGHT * 2f + ROW_SPACING * 3f + LayoutMetrics.COLOR_PICKER_HEIGHT + ColorPaletteSelector.TOTAL_HEIGHT,
            };

            _definitionDropdown = CreateFullWidthDropdown<DefinitionCatalog.BlockDefinitionOption>();
            foreach (var option in DefinitionCatalog.BlockDefinitions) {
                _definitionDropdown.Add(option.DisplayLabel, option);
            }

            _definitionTypeField = CreateFullWidthTextField();
            _definitionSubtypeField = CreateFullWidthTextField();

            _definitionSection = new HudChain(true) {
                CollectionContainer = {
                    CreateFullWidthCaption(ModText.BC_UI_ConditionHint_Definition.GetString()),
                    CreateFullWidthCaption(ModText.BC_UI_ConditionHint_Wildcards.GetString()),
                    _definitionDropdown,
                    CreateFieldRow("TypeId", _definitionTypeField, contentWidth),
                    CreateFieldRow("SubtypeId", _definitionSubtypeField, contentWidth)
                },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LABEL_HEIGHT * 2f + CONTROL_HEIGHT * 3f + ROW_SPACING * 4f,
            };

            _skinDropdown = DefinitionCatalog.CreateSkinDropdown(CONTROL_HEIGHT);

            _skinSection = new HudChain(true) {
                CollectionContainer = {
                    CreateFullWidthCaption(ModText.BC_UI_ConditionHint_Skin.GetString()),
                    _skinDropdown
                },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LABEL_HEIGHT + ROW_SPACING + CONTROL_HEIGHT,
            };

            _categoryDropdown = CreateFullWidthDropdown<PaintRuleBlockCategory>();
            _categoryDropdown.Add(ModText.BC_UI_Category_Armor.GetString(), PaintRuleBlockCategory.Armor);
            _categoryDropdown.Add(ModText.BC_UI_Category_LightArmor.GetString(), PaintRuleBlockCategory.LightArmor);
            _categoryDropdown.Add(ModText.BC_UI_Category_HeavyArmor.GetString(), PaintRuleBlockCategory.HeavyArmor);
            _categoryDropdown.Add(ModText.BC_UI_Category_Functional.GetString(), PaintRuleBlockCategory.Functional);

            _categorySection = new HudChain(true) {
                CollectionContainer = {
                    CreateFullWidthCaption(ModText.BC_UI_ConditionHint_Category.GetString()),
                    _categoryDropdown
                },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LABEL_HEIGHT + ROW_SPACING + CONTROL_HEIGHT,
            };

            _gridSizeDropdown = CreateFullWidthDropdown<PaintRuleGridSize>();
            _gridSizeDropdown.Add(ModText.BC_UI_GridSize_Large.GetString(), PaintRuleGridSize.Large);
            _gridSizeDropdown.Add(ModText.BC_UI_GridSize_Small.GetString(), PaintRuleGridSize.Small);

            _gridSizeSection = new HudChain(true) {
                CollectionContainer = {
                    CreateFullWidthCaption(ModText.BC_UI_ConditionHint_GridSize.GetString()),
                    _gridSizeDropdown
                },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LABEL_HEIGHT + ROW_SPACING + CONTROL_HEIGHT,
            };

            _integrityDropdown = CreateFullWidthDropdown<PaintRuleIntegrityState>();
            _integrityDropdown.Add(ModText.BC_UI_Integrity_Intact.GetString(), PaintRuleIntegrityState.Intact);
            _integrityDropdown.Add(ModText.BC_UI_Integrity_Damaged.GetString(), PaintRuleIntegrityState.Damaged);
            _integrityDropdown.Add(ModText.BC_UI_Integrity_Incomplete.GetString(), PaintRuleIntegrityState.Incomplete);
            _integrityDropdown.Add(ModText.BC_UI_Integrity_BelowThreshold.GetString(), PaintRuleIntegrityState.BelowThreshold);

            _integrityThresholdField = CreateFullWidthTextField();

            _integrityThresholdHint = CreateFullWidthCaption(ModText.BC_UI_ConditionHint_IntegrityThreshold.GetString());
            _integrityThresholdRow = CreateFieldRow(ModText.BC_UI_ThresholdPercent.GetString(), _integrityThresholdField, contentWidth);

            _integritySection = new HudChain(true) {
                CollectionContainer = {
                    CreateFullWidthCaption(ModText.BC_UI_ConditionHint_Integrity.GetString()),
                    _integrityDropdown,
                    _integrityThresholdHint,
                    _integrityThresholdRow
                },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LABEL_HEIGHT * 2f + CONTROL_HEIGHT * 2f + ROW_SPACING * 3f,
            };

            _anyBlockSection = new HudChain(true) {
                CollectionContainer = {
                    CreateFullWidthCaption(ModText.BC_UI_ConditionHint_AnyBlock.GetString()),
                    CreateFullWidthCaption(ModText.BC_UI_ConditionHint_AnyBlockOrder.GetString())
                },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LABEL_HEIGHT * 2f + ROW_SPACING,
            };

            _statusLabel = CreateFullWidthCaption(string.Empty, STATUS_HEIGHT);

            var cancelButton = CreateCancelButton(140f);
            var saveButton = CreateButton(ModText.BC_UI_Save.GetString(), 140f, ButtonRole.Primary);

            var buttonRow = new HudChain(false) {
                CollectionContainer = { cancelButton, saveButton },
                Spacing = ROW_SPACING,
                Width = contentWidth,
                Height = BUTTON_ROW_HEIGHT,
                SizingMode = HudChainSizingModes.AlignMembersEnd,
            };

            var sectionHost = new HudChain(true) {
                CollectionContainer = { _colorSection, _definitionSection, _skinSection, _categorySection, _gridSizeSection, _integritySection, _anyBlockSection },
                Spacing = 0f,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LayoutMetrics.CONDITION_SECTION_HEIGHT,
            };

            var layout = CreateContentColumn(ROW_SPACING);

            layout.Add(typeLabel, 0f);
            layout.Add(_conditionTypeDropdown, 0f);
            layout.Add(_comparisonLabel, 0f);
            layout.Add(_comparisonDropdown, 0f);
            layout.Add(CreateFullWidthSeparator(), 0f);
            layout.Add(sectionHost, 1f);
            layout.Add(_statusLabel, 0f);
            layout.Add(buttonRow, 0f);

            _conditionTypeDropdown.ValueChanged += OnConditionTypeChanged;
            _integrityDropdown.ValueChanged += OnIntegrityStateChanged;
            _definitionDropdown.ValueChanged += OnDefinitionSelected;
            saveButton.MouseInput.LeftClicked += OnSaveClicked;

            LoadCondition();
        }

        public event RichHudFramework.EventHandler Saved;

        public bool WasSaved { get; private set; }

        private static bool TryParseThreshold(string text, out float threshold) {
            return float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out threshold)
                && threshold >= 0f
                && threshold <= 100f;
        }

        private static HudChain CreateFieldRow(string caption, TextField field, float width) {
            var captionLabel = CreateCaption(caption, SECTION_LABEL_WIDTH, CONTROL_HEIGHT);

            return new HudChain(false) {
                CollectionContainer = { { captionLabel, 0f }, { field, 1f } },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = CONTROL_HEIGHT,
            };
        }

        private void LoadCondition() {
            _conditionTypeDropdown.SetSelection(_condition.Type);
            _comparisonDropdown.SetSelection(_condition.Comparison);

            _colorPicker.Color = _condition.Hsv;

            _definitionTypeField.Text = _condition.Definition.TypeId ?? string.Empty;
            _definitionSubtypeField.Text = _condition.Definition.SubtypeId ?? string.Empty;

            var skinIndex = DefinitionCatalog.IndexOfSkin(_condition.SkinId);
            _skinDropdown.SetSelectionAt(skinIndex >= 0 ? skinIndex : 0);

            _categoryDropdown.SetSelection(_condition.Category);
            _gridSizeDropdown.SetSelection(_condition.GridSize);

            _integrityDropdown.SetSelection(_condition.Integrity);
            _integrityThresholdField.Text = _condition.IntegrityThreshold.ToString("0.##");

            UpdateSectionVisibility();
        }

        private PaintRuleConditionType GetSelectedType() {
            return _conditionTypeDropdown.Value != null ? _conditionTypeDropdown.Value.AssocMember : PaintRuleConditionType.BlockColor;
        }

        private void UpdateSectionVisibility() {
            var type = GetSelectedType();

            _colorSection.Visible = type == PaintRuleConditionType.BlockColor;
            _definitionSection.Visible = type == PaintRuleConditionType.BlockDefinition;
            _skinSection.Visible = type == PaintRuleConditionType.BlockSkin;
            _categorySection.Visible = type == PaintRuleConditionType.BlockCategory;
            _gridSizeSection.Visible = type == PaintRuleConditionType.GridSize;
            _integritySection.Visible = type == PaintRuleConditionType.BlockIntegrity;
            _anyBlockSection.Visible = type == PaintRuleConditionType.AnyBlock;

            UpdateThresholdVisibility();

            var comparable = type != PaintRuleConditionType.AnyBlock;
            _comparisonLabel.Visible = comparable;
            _comparisonDropdown.Visible = comparable;
        }

        private PaintRuleIntegrityState GetSelectedIntegrityState() {
            return _integrityDropdown.Value != null ? _integrityDropdown.Value.AssocMember : PaintRuleIntegrityState.Damaged;
        }

        private void UpdateThresholdVisibility() {
            var showThreshold = GetSelectedIntegrityState() == PaintRuleIntegrityState.BelowThreshold;

            _integrityThresholdHint.Visible = showThreshold;
            _integrityThresholdRow.Visible = showThreshold;
        }

        private void OnIntegrityStateChanged(object sender, EventArgs e) {
            UpdateThresholdVisibility();
            _statusLabel.Text = string.Empty;
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnConditionTypeChanged(object sender, EventArgs e) {
            UpdateSectionVisibility();
            _statusLabel.Text = string.Empty;
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnPaletteColorPicked(ColorMask mask) {
            _colorPicker.Color = mask;
        }

        private void OnDefinitionSelected(object sender, EventArgs e) {
            var selected = _definitionDropdown.Value != null ? _definitionDropdown.Value.AssocMember : null;
            if (selected == null) {
                return;
            }

            _definitionTypeField.Text = selected.TypeId;
            _definitionSubtypeField.Text = selected.SubtypeId;
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

        private bool ApplyChanges() {
            var type = GetSelectedType();
            var typeId = _definitionTypeField.Text.ToString().Trim();
            var subtypeId = _definitionSubtypeField.Text.ToString().Trim();

            if (type == PaintRuleConditionType.BlockDefinition && string.IsNullOrEmpty(typeId) && string.IsNullOrEmpty(subtypeId)) {
                _statusLabel.Text = ModText.BC_UI_Status_DefinitionRequired.GetString();
                return false;
            }

            var integrityState = GetSelectedIntegrityState();
            var threshold = _condition.IntegrityThreshold;

            if (type == PaintRuleConditionType.BlockIntegrity && integrityState == PaintRuleIntegrityState.BelowThreshold
                && !TryParseThreshold(_integrityThresholdField.Text.ToString(), out threshold)) {
                _statusLabel.Text = ModText.BC_UI_Status_ThresholdRange.GetString();
                return false;
            }

            _condition.Type = type;

            _condition.Comparison = type != PaintRuleConditionType.AnyBlock && _comparisonDropdown.Value != null
                ? _comparisonDropdown.Value.AssocMember
                : PaintRuleComparison.Equals;

            _condition.Hsv = _colorPicker.Color;

            _condition.Definition = new PaintRuleDefinitionValue {
                TypeId = typeId,
                SubtypeId = subtypeId
            };

            var skin = _skinDropdown.Value != null ? _skinDropdown.Value.AssocMember : null;
            _condition.SkinId = skin != null ? skin.SkinId : string.Empty;

            if (_categoryDropdown.Value != null) {
                _condition.Category = _categoryDropdown.Value.AssocMember;
            }

            if (_gridSizeDropdown.Value != null) {
                _condition.GridSize = _gridSizeDropdown.Value.AssocMember;
            }

            _condition.Integrity = integrityState;
            _condition.IntegrityThreshold = threshold;

            _statusLabel.Text = string.Empty;
            return true;
        }
    }
}
