using RichHudFramework.UI;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using System;
using VRageMath;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Modal dialog for configuring a single paint rule condition.
    /// Only the section matching the selected condition type is shown.
    /// </summary>
    public class PaintRuleConditionDialog : DialogBase {
        private const float BUTTON_ROW_HEIGHT = LayoutMetrics.CONTROL_HEIGHT;
        private const float CONTROL_HEIGHT = LayoutMetrics.CONTROL_HEIGHT;
        private const float DIALOG_HEIGHT = 700f;

        /// <summary>
        /// Preferred width. The dialog shrinks to whatever the free screen region allows.
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

        private readonly HudChain _colorSection;
        private readonly ColorPickerHSV _colorPicker;
        private readonly ColorPaletteSelector _palette;

        private readonly HudChain _definitionSection;
        private readonly Dropdown<DefinitionCatalog.BlockDefinitionOption> _definitionDropdown;
        private readonly TextField _definitionTypeField;
        private readonly TextField _definitionSubtypeField;

        private readonly HudChain _skinSection;
        private readonly Dropdown<SkinListEntry, DefinitionCatalog.SkinOption> _skinDropdown;

        private readonly Label _statusLabel;

        public PaintRuleConditionDialog(PaintRuleCondition condition, HudParentBase parent = null) : base(parent) {
            _condition = condition ?? new PaintRuleCondition();

            var dialogWidth = MathHelper.Clamp(DialogSafeArea.GetAvailableWidth(), MIN_DIALOG_WIDTH, PREFERRED_DIALOG_WIDTH);

            Size = new Vector2(dialogWidth, DIALOG_HEIGHT);
            HeaderText = "Condition Editor";

            var contentWidth = dialogWidth - Padding.X - LayoutMetrics.CONTENT_PADDING_X;

            var typeLabel = CreateLabel("Condition Type");
            _conditionTypeDropdown = new Dropdown<PaintRuleConditionType>() { DimAlignment = DimAlignments.Width, Height = CONTROL_HEIGHT };
            _conditionTypeDropdown.Add("Block Color", PaintRuleConditionType.BlockColor);
            _conditionTypeDropdown.Add("Block Definition", PaintRuleConditionType.BlockDefinition);
            _conditionTypeDropdown.Add("Block Skin", PaintRuleConditionType.BlockSkin);

            var comparisonLabel = CreateLabel("Comparison");
            _comparisonDropdown = new Dropdown<PaintRuleComparison>() { DimAlignment = DimAlignments.Width, Height = CONTROL_HEIGHT };
            _comparisonDropdown.Add("Matches", PaintRuleComparison.Equals);
            _comparisonDropdown.Add("Does not match", PaintRuleComparison.NotEquals);

            // Block color section
            _colorPicker = new ColorPickerHSV() {
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.COLOR_PICKER_HEIGHT,
                Name = "Block Color",
            };

            _palette = new ColorPaletteSelector() { DimAlignment = DimAlignments.Width };
            _palette.ColorPicked += OnPaletteColorPicked;

            _colorSection = new HudChain(true) {
                CollectionContainer = {
                    CreateLabel("The rule matches blocks painted with this color."),
                    _colorPicker,
                    CreateLabel("Or pick from your current build palette:"),
                    _palette
                },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LABEL_HEIGHT * 2f + ROW_SPACING * 3f + LayoutMetrics.COLOR_PICKER_HEIGHT + ColorPaletteSelector.TOTAL_HEIGHT,
            };

            // Block definition section
            _definitionDropdown = new Dropdown<DefinitionCatalog.BlockDefinitionOption>() { DimAlignment = DimAlignments.Width, Height = CONTROL_HEIGHT };
            foreach (var option in DefinitionCatalog.BlockDefinitions) {
                _definitionDropdown.Add(option.DisplayLabel, option);
            }

            _definitionTypeField = new GameInputBlockingTextField() { DimAlignment = DimAlignments.Width, Height = CONTROL_HEIGHT };
            _definitionSubtypeField = new GameInputBlockingTextField() { DimAlignment = DimAlignments.Width, Height = CONTROL_HEIGHT };

            _definitionSection = new HudChain(true) {
                CollectionContainer = {
                    CreateLabel("Pick a block, or leave a field empty to match any value."),
                    _definitionDropdown,
                    CreateFieldRow("TypeId", _definitionTypeField, contentWidth),
                    CreateFieldRow("SubtypeId", _definitionSubtypeField, contentWidth)
                },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LABEL_HEIGHT + CONTROL_HEIGHT * 3f + ROW_SPACING * 3f,
            };

            // Block skin section
            _skinDropdown = DefinitionCatalog.CreateSkinDropdown(CONTROL_HEIGHT);

            _skinSection = new HudChain(true) {
                CollectionContainer = {
                    CreateLabel("The rule matches blocks using this armor skin."),
                    _skinDropdown
                },
                Spacing = ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LABEL_HEIGHT + ROW_SPACING + CONTROL_HEIGHT,
            };

            _statusLabel = new Label() {
                Text = string.Empty,
                Format = Style.BodyText,
                AutoResize = false,
                DimAlignment = DimAlignments.Width,
                Height = STATUS_HEIGHT,
            };

            var saveButton = new BorderedButton() { Text = "Save", Padding = Vector2.Zero, Width = 140f, Height = BUTTON_ROW_HEIGHT };
            var cancelButton = new BorderedButton() { Text = "Cancel", Padding = Vector2.Zero, Width = 140f, Height = BUTTON_ROW_HEIGHT };

            var buttonRow = new HudChain(false) {
                CollectionContainer = { saveButton, cancelButton },
                Spacing = ROW_SPACING,
                Width = contentWidth,
                Height = BUTTON_ROW_HEIGHT,
                SizingMode = HudChainSizingModes.AlignMembersEnd,
            };

            // The sections are stacked in the same slot; only one is visible at a time.
            var sectionHost = new HudChain(true) {
                CollectionContainer = { _colorSection, _definitionSection, _skinSection },
                Spacing = 0f,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LayoutMetrics.CONDITION_SECTION_HEIGHT,
            };

            var layout = new HudChain(true, body) {
                ParentAlignment = ParentAlignments.Inner,
                DimAlignment = DimAlignments.UnpaddedSize,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                CollectionContainer = {
                    typeLabel,
                    _conditionTypeDropdown,
                    comparisonLabel,
                    _comparisonDropdown,
                    CreateSeparator(),
                    { sectionHost, 1f },
                    _statusLabel,
                    buttonRow
                },
                Spacing = ROW_SPACING,
                Padding = new Vector2(LayoutMetrics.CONTENT_PADDING_X, LayoutMetrics.CONTENT_PADDING_Y)
            };

            _conditionTypeDropdown.ValueChanged += OnConditionTypeChanged;
            _definitionDropdown.ValueChanged += OnDefinitionSelected;
            _definitionDropdown.MouseInput.CursorEntered += OnMouseOver;
            _skinDropdown.MouseInput.CursorEntered += OnMouseOver;
            _conditionTypeDropdown.MouseInput.CursorEntered += OnMouseOver;
            _comparisonDropdown.MouseInput.CursorEntered += OnMouseOver;
            saveButton.MouseInput.LeftClicked += OnSaveClicked;
            saveButton.MouseInput.CursorEntered += OnMouseOver;
            cancelButton.MouseInput.LeftClicked += OnCancelClicked;
            cancelButton.MouseInput.CursorEntered += OnMouseOver;

            LoadCondition();
        }

        public event RichHudFramework.EventHandler Saved;

        public bool WasSaved { get; private set; }

        private static Label CreateLabel(string text) {
            return new Label() {
                Text = text,
                Format = Style.BodyText,
                AutoResize = false,
                DimAlignment = DimAlignments.Width,
                Height = LABEL_HEIGHT,
            };
        }

        private static TexturedBox CreateSeparator() {
            return new TexturedBox() { DimAlignment = DimAlignments.Width, Height = .75f, Color = Style.SeparatorColor };
        }

        private static HudChain CreateFieldRow(string caption, TextField field, float width) {
            var captionLabel = new Label() {
                Text = caption,
                Format = Style.BodyText,
                AutoResize = false,
                Width = SECTION_LABEL_WIDTH,
                Height = CONTROL_HEIGHT,
            };

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

            var color = _condition.Color.Value;
            _colorPicker.Value = new VRageMath.Color(color.R, color.G, color.B);

            _definitionTypeField.Text = _condition.Definition.TypeId ?? string.Empty;
            _definitionSubtypeField.Text = _condition.Definition.SubtypeId ?? string.Empty;

            var skinIndex = DefinitionCatalog.IndexOfSkin(_condition.Skin.SkinId);
            _skinDropdown.SetSelectionAt(skinIndex >= 0 ? skinIndex : 0);

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
        }

        private void OnConditionTypeChanged(object sender, EventArgs e) {
            UpdateSectionVisibility();
            _statusLabel.Text = string.Empty;
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnPaletteColorPicked(VRageMath.Color color) {
            _colorPicker.Value = color;
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

        private void OnCancelClicked(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudLockingLost");
            Close();
        }

        private void OnMouseOver(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudMouseOver");
        }

        private bool ApplyChanges() {
            var type = GetSelectedType();
            var typeId = _definitionTypeField.Text.ToString().Trim();
            var subtypeId = _definitionSubtypeField.Text.ToString().Trim();

            if (type == PaintRuleConditionType.BlockDefinition && string.IsNullOrEmpty(typeId) && string.IsNullOrEmpty(subtypeId)) {
                _statusLabel.Text = "Set a TypeId or SubtypeId, otherwise the condition matches every block.";
                return false;
            }

            _condition.Type = type;

            if (_comparisonDropdown.Value != null) {
                _condition.Comparison = _comparisonDropdown.Value.AssocMember;
            }

            var color = _colorPicker.Value;
            _condition.Color = new PaintRuleColorValue {
                Enabled = type == PaintRuleConditionType.BlockColor,
                Value = new ColorModel(color.R, color.G, color.B)
            };

            _condition.Definition = new PaintRuleDefinitionValue {
                Enabled = type == PaintRuleConditionType.BlockDefinition,
                TypeId = typeId,
                SubtypeId = subtypeId
            };

            var skin = _skinDropdown.Value != null ? _skinDropdown.Value.AssocMember : null;
            _condition.Skin = new PaintRuleSkinValue {
                Enabled = type == PaintRuleConditionType.BlockSkin,
                SkinId = skin != null ? skin.SkinId : string.Empty
            };

            _statusLabel.Text = string.Empty;
            return true;
        }
    }
}
