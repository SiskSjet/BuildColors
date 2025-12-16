using RichHudFramework.UI;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using System;
using VRageMath;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Dialog for editing a paint job: its name, options, rules and the action of the selected rule.
    /// The conditions of a rule are edited in a separate dialog.
    /// </summary>
    public class PaintJobEditorDialog : DialogBase {
        private const float COLUMN_SPACING = 16f;
        private const int COLUMN_COUNT = 2;
        private const float DIALOG_HEIGHT = 1000f;
        private const float MIN_DIALOG_WIDTH = 688f;
        private const float PREFERRED_DIALOG_WIDTH = 900f;

        private readonly PaintJob _job;

        private readonly TextField _jobNameField;
        private readonly BorderedCheckBox _includePreviewCheckbox;
        private readonly BorderedCheckBox _includeProjectedCheckbox;
        private readonly BorderedCheckBox _includeSubgridsCheckbox;
        private readonly BorderedCheckBox _respectOwnershipCheckbox;

        private readonly ListBox<PaintRule> _ruleList;
        private readonly BorderedButton _removeRuleButton;
        private readonly BorderedButton _moveRuleUpButton;
        private readonly BorderedButton _moveRuleDownButton;

        private readonly TextField _ruleNameField;
        private readonly Label _conditionSummaryLabel;
        private readonly BorderedButton _editConditionsButton;

        private readonly BorderedCheckBox _actionApplyColorCheckbox;
        private readonly ColorPickerHSV _actionColorPicker;
        private readonly ColorPaletteSelector _actionPalette;
        private readonly BorderedCheckBox _actionApplySkinCheckbox;
        private readonly Dropdown<SkinListEntry, DefinitionCatalog.SkinOption> _actionSkinDropdown;

        private readonly Label _statusLabel;

        private PaintRuleConditionGroupDialog _activeConditionsDialog;
        private PaintRule _conditionsDialogRule;
        private PaintRule _loadedRule;

        public PaintJobEditorDialog(PaintJob job, HudParentBase parent = null) : base(parent) {
            // Everything is edited on a working copy, so Cancel simply discards it and the stored job is
            // only replaced when Save commits the copy.
            _job = job.Clone();
            _job.EnsureRules();

            var dialogWidth = MathHelper.Clamp(DialogSafeArea.GetAvailableWidth(), MIN_DIALOG_WIDTH, PREFERRED_DIALOG_WIDTH);

            Size = new Vector2(dialogWidth, DIALOG_HEIGHT);
            HeaderText = "Paint Job Editor";

            var contentWidth = dialogWidth - Padding.X - LayoutMetrics.CONTENT_PADDING_X;
            var contentHeight = DIALOG_HEIGHT - Padding.Y - HEADER_HEIGHT - LayoutMetrics.CONTENT_PADDING_Y;
            var columnWidth = (contentWidth - COLUMN_SPACING * (COLUMN_COUNT - 1)) / COLUMN_COUNT;
            var columnHeight = contentHeight
                - LayoutMetrics.STATUS_HEIGHT
                - LayoutMetrics.BUTTON_HEIGHT
                - LayoutMetrics.SECTION_SPACING * 2f;

            // Column 1 - job level settings and the rule list
            _jobNameField = new TextField() { DimAlignment = DimAlignments.Width, Height = LayoutMetrics.CONTROL_HEIGHT };

            _includeSubgridsCheckbox = new BorderedCheckBox();
            _includeProjectedCheckbox = new BorderedCheckBox();
            _includePreviewCheckbox = new BorderedCheckBox();
            _respectOwnershipCheckbox = new BorderedCheckBox();

            var optionsLayout = new HudChain(true) {
                CollectionContainer = {
                    CreateCheckboxRow(_includeSubgridsCheckbox, "Include subgrids", columnWidth),
                    CreateCheckboxRow(_includeProjectedCheckbox, "Include projected grids", columnWidth),
                    CreateCheckboxRow(_includePreviewCheckbox, "Include preview grids", columnWidth),
                    CreateCheckboxRow(_respectOwnershipCheckbox, "Respect ownership", columnWidth)
                },
                Spacing = 6f,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = columnWidth,
                Height = LayoutMetrics.CHECKBOX_SIZE * 4f + 6f * 3f,
            };

            _ruleList = new ListBox<PaintRule>() { DimAlignment = DimAlignments.Width };

            var addRuleButton = CreateButton("Add Rule");
            _removeRuleButton = CreateButton("Remove Rule");

            var ruleButtons = new HudChain(false) {
                CollectionContainer = { { addRuleButton, 1f }, { _removeRuleButton, 1f } },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = columnWidth,
                Height = LayoutMetrics.BUTTON_HEIGHT,
            };

            _moveRuleUpButton = CreateButton("Move Up");
            _moveRuleDownButton = CreateButton("Move Down");

            var ruleMoveButtons = new HudChain(false) {
                CollectionContainer = { { _moveRuleUpButton, 1f }, { _moveRuleDownButton, 1f } },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = columnWidth,
                Height = LayoutMetrics.BUTTON_HEIGHT,
            };

            var jobColumn = CreateColumn(columnWidth, columnHeight);
            jobColumn.Add(CreateLabel("Job Name"), 0f);
            jobColumn.Add(_jobNameField, 0f);
            jobColumn.Add(CreateSeparator(), 0f);
            jobColumn.Add(CreateLabel("Options"), 0f);
            jobColumn.Add(optionsLayout, 0f);
            jobColumn.Add(CreateSeparator(), 0f);
            jobColumn.Add(CreateLabel("Rules"), 0f);
            jobColumn.Add(CreateLabel("Applied in order; the first matching rule wins."), 0f);
            jobColumn.Add(_ruleList, 1f);
            jobColumn.Add(ruleButtons, 0f);
            jobColumn.Add(ruleMoveButtons, 0f);

            // Column 2 - the selected rule
            _ruleNameField = new TextField() { DimAlignment = DimAlignments.Width, Height = LayoutMetrics.CONTROL_HEIGHT };
            _conditionSummaryLabel = CreateLabel(string.Empty);
            _editConditionsButton = CreateButton("Edit Conditions...");

            // Action applied to matching blocks
            _actionApplyColorCheckbox = new BorderedCheckBox();
            _actionColorPicker = new ColorPickerHSV() {
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.COLOR_PICKER_HEIGHT,
                Name = "Target Color",
            };

            _actionPalette = new ColorPaletteSelector() { DimAlignment = DimAlignments.Width };
            _actionPalette.ColorPicked += OnPaletteColorPicked;

            _actionApplySkinCheckbox = new BorderedCheckBox();
            _actionSkinDropdown = DefinitionCatalog.CreateSkinDropdown(LayoutMetrics.CONTROL_HEIGHT);

            // Column 2 holds everything about the selected rule. Members stack from the top, so any
            // leftover height simply stays empty.
            var ruleColumn = CreateColumn(columnWidth, columnHeight);
            ruleColumn.Add(CreateLabel("Rule Details"), 0f);
            ruleColumn.Add(CreateLabel("Rule Name"), 0f);
            ruleColumn.Add(_ruleNameField, 0f);
            ruleColumn.Add(CreateSeparator(), 0f);
            ruleColumn.Add(CreateLabel("When"), 0f);
            ruleColumn.Add(_conditionSummaryLabel, 0f);
            ruleColumn.Add(_editConditionsButton, 0f);
            ruleColumn.Add(CreateSeparator(), 0f);
            ruleColumn.Add(CreateLabel("Then paint with"), 0f);
            ruleColumn.Add(CreateCheckboxRow(_actionApplyColorCheckbox, "Apply color", columnWidth), 0f);
            ruleColumn.Add(_actionColorPicker, 0f);
            ruleColumn.Add(CreateLabel("Or pick from your current build palette:"), 0f);
            ruleColumn.Add(_actionPalette, 0f);
            ruleColumn.Add(CreateCheckboxRow(_actionApplySkinCheckbox, "Apply skin", columnWidth), 0f);
            ruleColumn.Add(CreateLabel("Target Skin"), 0f);
            ruleColumn.Add(_actionSkinDropdown, 0f);

            var mainColumns = new HudChain(false) {
                CollectionContainer = { jobColumn, ruleColumn },
                Spacing = COLUMN_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = columnHeight,
            };

            _statusLabel = new Label() {
                Text = string.Empty,
                Format = Style.BodyText,
                AutoResize = false,
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.STATUS_HEIGHT,
            };

            var saveButton = CreateButton("Save");
            saveButton.Width = 150f;

            var cancelButton = CreateButton("Cancel");
            cancelButton.Width = 150f;

            var buttonRow = new HudChain(false) {
                CollectionContainer = { saveButton, cancelButton },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.AlignMembersEnd,
                Width = contentWidth,
                Height = LayoutMetrics.BUTTON_HEIGHT,
            };

            var mainLayout = new HudChain(true, body) {
                ParentAlignment = ParentAlignments.Inner,
                DimAlignment = DimAlignments.UnpaddedSize,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                CollectionContainer = { { mainColumns, 1f }, _statusLabel, buttonRow },
                Spacing = LayoutMetrics.SECTION_SPACING,
                Padding = new Vector2(LayoutMetrics.CONTENT_PADDING_X, LayoutMetrics.CONTENT_PADDING_Y)
            };

            _ruleList.ValueChanged += OnRuleSelectionChanged;

            addRuleButton.MouseInput.LeftClicked += OnAddRule;
            addRuleButton.MouseInput.CursorEntered += OnMouseOver;
            _removeRuleButton.MouseInput.LeftClicked += OnRemoveRule;
            _removeRuleButton.MouseInput.CursorEntered += OnMouseOver;
            _moveRuleUpButton.MouseInput.LeftClicked += (s2, e2) => MoveSelectedRule(-1);
            _moveRuleUpButton.MouseInput.CursorEntered += OnMouseOver;
            _moveRuleDownButton.MouseInput.LeftClicked += (s2, e2) => MoveSelectedRule(1);
            _moveRuleDownButton.MouseInput.CursorEntered += OnMouseOver;

            _editConditionsButton.MouseInput.LeftClicked += OnEditConditions;
            _editConditionsButton.MouseInput.CursorEntered += OnMouseOver;

            saveButton.MouseInput.LeftClicked += OnSaveClicked;
            saveButton.MouseInput.CursorEntered += OnMouseOver;
            cancelButton.MouseInput.LeftClicked += OnCancelClicked;
            cancelButton.MouseInput.CursorEntered += OnMouseOver;

            _jobNameField.MouseInput.CursorEntered += OnMouseOver;
            _includeSubgridsCheckbox.MouseInput.CursorEntered += OnMouseOver;
            _includeProjectedCheckbox.MouseInput.CursorEntered += OnMouseOver;
            _includePreviewCheckbox.MouseInput.CursorEntered += OnMouseOver;
            _respectOwnershipCheckbox.MouseInput.CursorEntered += OnMouseOver;
            _actionApplyColorCheckbox.MouseInput.CursorEntered += OnMouseOver;
            _actionApplySkinCheckbox.MouseInput.CursorEntered += OnMouseOver;
            _actionSkinDropdown.MouseInput.CursorEntered += OnMouseOver;

            LoadRule();
        }

        public event RichHudFramework.EventHandler Saved;

        /// <summary>
        /// Event args for paint job save events
        /// </summary>
        public class PaintJobEventArgs : EventArgs {
            public PaintJob Job { get; set; }
        }

        private static HudChain CreateColumn(float width, float height) {
            return new HudChain(true) {
                Spacing = LayoutMetrics.SECTION_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = height,
            };
        }

        private static Label CreateLabel(string text) {
            return new Label() {
                Text = text,
                Format = Style.BodyText,
                AutoResize = false,
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.LABEL_HEIGHT,
            };
        }

        private static TexturedBox CreateSeparator() {
            return new TexturedBox() { DimAlignment = DimAlignments.Width, Height = LayoutMetrics.SEPARATOR_HEIGHT, Color = Style.SeparatorColor };
        }

        private static BorderedButton CreateButton(string text) {
            return new BorderedButton() { Text = text, Padding = Vector2.Zero, Height = LayoutMetrics.BUTTON_HEIGHT };
        }

        private static HudChain CreateCheckboxRow(BorderedCheckBox checkbox, string text, float width) {
            var label = new Label() {
                Text = text,
                Format = Style.BodyText,
                AutoResize = false,
                Height = LayoutMetrics.CHECKBOX_SIZE,
            };

            return new HudChain(false) {
                CollectionContainer = { { checkbox, 0f }, { label, 1f } },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = LayoutMetrics.CHECKBOX_SIZE,
            };
        }

        private void LoadRule() {
            _job.EnsureRules();

            _jobNameField.Text = _job.Name;
            if (_job.Options == null) {
                _job.Options = new PaintJobOptions();
            }

            _includeSubgridsCheckbox.Value = _job.Options.IncludeSubgrids;
            _includeProjectedCheckbox.Value = _job.Options.IncludeProjectedGrids;
            _includePreviewCheckbox.Value = _job.Options.IncludePreviewGrids;
            _respectOwnershipCheckbox.Value = _job.Options.RespectOwnership;

            RefreshRules();
        }

        private void RefreshRules(PaintRule ruleToSelect = null) {
            _ruleList.ClearEntries();

            var rules = _job.Rules;
            if (rules != null) {
                foreach (var rule in rules) {
                    _ruleList.Add(rule.Name, rule);
                }
            }

            if (_ruleList.Count > 0) {
                var index = ruleToSelect != null && rules != null ? rules.IndexOf(ruleToSelect) : 0;
                _ruleList.SetSelectionAt(index >= 0 && index < _ruleList.Count ? index : 0);
            } else {
                _loadedRule = null;
                ClearRuleEditor();
            }

            UpdateRuleButtonState();
        }

        private void OnRuleSelectionChanged(object sender, EventArgs e) {
            // Flush pending edits of the rule that was selected before this change.
            SaveRuleData(_loadedRule);
            SyncRuleLabel(_loadedRule);

            var rule = GetSelectedRule();
            _loadedRule = rule;

            if (rule == null) {
                ClearRuleEditor();
                UpdateRuleButtonState();
                return;
            }

            _ruleNameField.Text = rule.Name;

            LoadAction(rule);
            UpdateConditionSummary(rule);
            UpdateRuleButtonState();
        }

        private void UpdateConditionSummary(PaintRule rule) {
            _conditionSummaryLabel.Text = rule != null
                ? PaintRuleConditionText.DescribeGroup(rule.ConditionGroup)
                : string.Empty;
        }

        private void OnAddRule(object sender, EventArgs e) {
            SaveRuleData(_loadedRule);

            var index = _job.Rules.Count + 1;
            var name = string.Format("Rule {0}", index);
            while (_job.Rules.Exists(rule => string.Equals(rule.Name, name, StringComparison.InvariantCultureIgnoreCase))) {
                index++;
                name = string.Format("Rule {0}", index);
            }

            var newRule = PaintRule.CreateDefault(name);
            _job.Rules.Add(newRule);

            RefreshRules(newRule);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnRemoveRule(object sender, EventArgs e) {
            var rule = GetSelectedRule();
            if (rule == null) {
                return;
            }

            if (_job.Rules.Count <= 1) {
                _statusLabel.Text = "A paint job needs at least one rule.";
                HudSoundUtils.PlaySound("HudLockingLost");
                return;
            }

            _job.Rules.Remove(rule);
            _loadedRule = null;

            RefreshRules();
            HudSoundUtils.PlaySound("HudLockingLost");
        }

        private void OnEditConditions(object sender, EventArgs e) {
            var rule = GetSelectedRule();
            if (rule == null || _activeConditionsDialog != null) {
                return;
            }

            // The name is shown in the conditions dialog header, so commit it first.
            SaveRuleData(rule);
            SyncRuleLabel(rule);

            _conditionsDialogRule = rule;
            _activeConditionsDialog = new PaintRuleConditionGroupDialog(rule);
            _activeConditionsDialog.Saved += OnConditionsDialogSaved;
            _activeConditionsDialog.Closed += OnConditionsDialogClosed;

            RequestDialog(_activeConditionsDialog);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnConditionsDialogSaved(object sender, EventArgs e) {
            if (_conditionsDialogRule != null && _conditionsDialogRule == GetSelectedRule()) {
                UpdateConditionSummary(_conditionsDialogRule);
            }
        }

        private void OnConditionsDialogClosed(object sender, EventArgs e) {
            if (_activeConditionsDialog != null) {
                _activeConditionsDialog.Saved -= OnConditionsDialogSaved;
                _activeConditionsDialog.Closed -= OnConditionsDialogClosed;
            }

            _activeConditionsDialog = null;
            _conditionsDialogRule = null;

            UpdateConditionSummary(GetSelectedRule());
        }

        private void LoadAction(PaintRule rule) {
            if (rule == null) {
                return;
            }

            if (rule.Action == null) {
                rule.Action = new PaintRuleAction();
            }

            _actionApplyColorCheckbox.Value = rule.Action.ApplyColor;

            var color = rule.Action.TargetColor;
            _actionColorPicker.Value = new VRageMath.Color(color.R, color.G, color.B);

            _actionApplySkinCheckbox.Value = rule.Action.ApplySkin;

            var skinIndex = DefinitionCatalog.IndexOfSkin(rule.Action.TargetSkin.SkinId);
            _actionSkinDropdown.SetSelectionAt(skinIndex >= 0 ? skinIndex : 0);
        }

        private void ClearRuleEditor() {
            _ruleNameField.Text = string.Empty;
            _conditionSummaryLabel.Text = string.Empty;
        }

        private void SaveRuleData(PaintRule rule) {
            if (rule == null || !_job.Rules.Contains(rule)) {
                return;
            }

            var name = _ruleNameField.Text.ToString().Trim();
            if (!string.IsNullOrEmpty(name)) {
                rule.Name = name;
            }

            if (rule.ConditionGroup == null) {
                rule.ConditionGroup = PaintRuleConditionGroup.CreateDefault();
            }

            if (rule.Action == null) {
                rule.Action = new PaintRuleAction();
            }

            rule.Action.ApplyColor = _actionApplyColorCheckbox.Value;

            var actionColor = _actionColorPicker.Value;
            rule.Action.TargetColor = new ColorModel(actionColor.R, actionColor.G, actionColor.B);

            rule.Action.ApplySkin = _actionApplySkinCheckbox.Value;

            var skin = _actionSkinDropdown.Value != null ? _actionSkinDropdown.Value.AssocMember : null;
            rule.Action.TargetSkin = new PaintRuleSkinValue {
                SkinId = skin != null ? skin.SkinId : string.Empty,
                Enabled = rule.Action.ApplySkin
            };
        }

        /// <summary>
        /// Pushes a renamed rule back into its list row so the list does not show a stale name.
        /// </summary>
        private void SyncRuleLabel(PaintRule rule) {
            if (rule == null) {
                return;
            }

            var index = _job.Rules.IndexOf(rule);
            if (index < 0 || index >= _ruleList.EntryList.Count) {
                return;
            }

            _ruleList.EntryList[index].Element.TextBoard.SetText(rule.Name);
        }

        private PaintRule GetSelectedRule() {
            return _ruleList.Value != null ? _ruleList.Value.AssocMember : null;
        }

        private void UpdateRuleButtonState() {
            var rule = GetSelectedRule();
            var hasRule = rule != null;
            var index = hasRule ? _job.Rules.IndexOf(rule) : -1;

            _removeRuleButton.InputEnabled = _job.Rules.Count > 1 && hasRule;
            _editConditionsButton.InputEnabled = hasRule;
            _moveRuleUpButton.InputEnabled = index > 0;
            _moveRuleDownButton.InputEnabled = index >= 0 && index < _job.Rules.Count - 1;
        }

        /// <summary>
        /// Reorders the selected rule. Order decides which rule wins, so this changes what the job paints.
        /// </summary>
        private void MoveSelectedRule(int offset) {
            var rule = GetSelectedRule();
            if (rule == null) {
                return;
            }

            var index = _job.Rules.IndexOf(rule);
            var target = index + offset;

            if (index < 0 || target < 0 || target >= _job.Rules.Count) {
                return;
            }

            // Flush pending edits first, otherwise rebuilding the list would discard them.
            SaveRuleData(_loadedRule);

            _job.Rules.RemoveAt(index);
            _job.Rules.Insert(target, rule);

            RefreshRules(rule);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnSaveClicked(object sender, EventArgs e) {
            SaveRuleData(GetSelectedRule());

            var name = _jobNameField.Text.ToString().Trim();
            if (string.IsNullOrEmpty(name)) {
                _statusLabel.Text = "Job name cannot be empty.";
                HudSoundUtils.PlaySound("HudLockingLost");
                return;
            }

            _job.Name = name;
            if (_job.Options == null) {
                _job.Options = new PaintJobOptions();
            }

            _job.Options.IncludeSubgrids = _includeSubgridsCheckbox.Value;
            _job.Options.IncludeProjectedGrids = _includeProjectedCheckbox.Value;
            _job.Options.IncludePreviewGrids = _includePreviewCheckbox.Value;
            _job.Options.RespectOwnership = _respectOwnershipCheckbox.Value;

            HudSoundUtils.PlaySound("HudBleep");
            Saved?.Invoke(this, new PaintJobEventArgs { Job = _job });
            Close();
        }

        private void OnCancelClicked(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudLockingLost");
            Close();
        }

        private void OnPaletteColorPicked(VRageMath.Color color) {
            _actionColorPicker.Value = color;
        }

        private void OnMouseOver(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudMouseOver");
        }
    }
}
