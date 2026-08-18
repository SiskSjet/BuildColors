using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Services;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Linq;
using VRageMath;


namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Paint jobs, the rules of the selected job and the detail of the selected rule, side by side.
    /// </summary>
    internal class PaintJobsView : PanelView {
        /// <summary>
        /// Below this the lists move into a row above the rule detail instead of standing beside it, since
        /// the detail is the only pane holding more than a list.
        /// </summary>
        private const float INSPECTOR_MIN_WIDTH = 420f;

        /// <summary>
        /// Height the lists keep once they stand above the rule detail.
        /// </summary>
        private const float STACKED_LIST_MIN_HEIGHT = 340f;

        private const float APPLY_BUTTON_WIDTH = 280f;
        private const float JOBS_PANE_WIDTH = 300f;
        private const float RULES_PANE_WIDTH = 340f;

        private readonly ListBox<PaintJob> _jobList;
        private readonly TextField _jobNameField;
        private readonly ActionButton _copyJobButton;
        private readonly ActionButton _inboxButton;
        private readonly ActionButton _removeJobButton;
        private readonly ActionButton _shareJobButton;

        private readonly Card _rulesCard;
        private readonly ListBox<PaintRule> _ruleList;
        private readonly TextField _ruleNameField;
        private readonly ActionButton _addRuleButton;
        private readonly ActionButton _removeRuleButton;
        private readonly ActionButton _moveRuleUpButton;
        private readonly ActionButton _moveRuleDownButton;

        private readonly Card _inspectorCard;
        private readonly ActionButton _conditionsTabButton;
        private readonly ActionButton _paintTabButton;

        private readonly HudChain _conditionsView;
        private readonly ListBox<PaintRuleNode> _conditionTree;
        private readonly ActionButton _editConditionsButton;

        private readonly HudChain _paintView;
        private readonly BorderedCheckBox _applyColorCheckbox;
        private readonly BorderedCheckBox _applySkinCheckbox;
        private readonly Dropdown<PaintSourceType> _sourceTypeDropdown;
        private readonly HudChain _solidSection;
        private readonly ColorPickerHSV2 _colorPicker;
        private readonly ColorPaletteSelector _palette;
        private readonly Dropdown<SkinListEntry, DefinitionCatalog.SkinOption> _skinDropdown;
        private readonly HudChain _sourceSection;
        private readonly Label _sourceSummaryLabel;
        private readonly ActionButton _editSourceButton;

        private readonly BorderedCheckBox _includeSubgridsCheckbox;
        private readonly BorderedCheckBox _includeProjectedCheckbox;
        private readonly BorderedCheckBox _includePreviewCheckbox;

        private readonly ActionButton _applyButton;
        private readonly ActionButton _undoButton;
        private readonly Label _hotkeyHint;

        private PaintJob _loadedJob;
        private PaintRule _loadedRule;
        private bool _suppressWrites;

        public PaintJobsView(float width, float height, HudParentBase parent = null) : base(width, height, parent) {
            var optionsHeight = LayoutMetrics.HEADING_HEIGHT + LayoutMetrics.CHECKBOX_SIZE + LayoutMetrics.TIGHT_SPACING;
            var paneHeight = height
                - LayoutMetrics.BUTTON_HEIGHT
                - optionsHeight
                - LayoutMetrics.SECTION_SPACING * 2f;

            _applyButton = ControlFactory.CreateButton(ModText.BC_UI_ApplyToTargetGrid.GetString(), APPLY_BUTTON_WIDTH, ButtonRole.Primary);
            _undoButton = ControlFactory.CreateButton(ModText.BC_UI_Undo.GetString(), 160f);

            _hotkeyHint = ControlFactory.CreateCaption(
                string.Empty,
                width - APPLY_BUTTON_WIDTH - _undoButton.Width - LayoutMetrics.ROW_SPACING * 2f,
                LayoutMetrics.BUTTON_HEIGHT);

            _hotkeyHint.VertCenterText = true;

            var toolbar = new HudChain(false) {
                CollectionContainer = { _hotkeyHint, _undoButton, _applyButton },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.AlignMembersEnd,
                Width = width,
                Height = LayoutMetrics.BUTTON_HEIGHT,
            };

            var paintChrome = LayoutMetrics.CHECKBOX_SIZE + LayoutMetrics.CONTROL_HEIGHT + LayoutMetrics.ROW_SPACING * 2f;
            var stackHeight = LayoutMetrics.COLOR_PICKER_HEIGHT + LayoutMetrics.CONTROL_HEIGHT + LayoutMetrics.ROW_SPACING;
            var paletteCost = ColorPaletteSelector.TOTAL_HEIGHT + LayoutMetrics.LABEL_HEIGHT + LayoutMetrics.ROW_SPACING * 2f;
            var tabChrome = LayoutMetrics.BUTTON_HEIGHT + LayoutMetrics.ROW_SPACING;

            var sideBySideInspectorWidth = width - JOBS_PANE_WIDTH - RULES_PANE_WIDTH - LayoutMetrics.SECTION_SPACING * 2f;
            var wide = sideBySideInspectorWidth >= INSPECTOR_MIN_WIDTH;

            float jobsWidth;
            float rulesWidth;
            float inspectorWidth;
            float jobsHeight;
            float rulesHeight;
            float inspectorHeight;

            if (wide) {
                jobsWidth = JOBS_PANE_WIDTH;
                rulesWidth = RULES_PANE_WIDTH;
                inspectorWidth = sideBySideInspectorWidth;
                jobsHeight = paneHeight;
                rulesHeight = paneHeight;
                inspectorHeight = paneHeight;
            } else {
                var wanted = Card.HeightFor(tabChrome + paintChrome + stackHeight + paletteCost);
                var needed = Card.HeightFor(tabChrome + paintChrome + stackHeight);

                jobsWidth = (float)Math.Floor((width - LayoutMetrics.SECTION_SPACING) * .5f);
                rulesWidth = width - jobsWidth - LayoutMetrics.SECTION_SPACING;
                inspectorWidth = width;
                inspectorHeight = MathHelper.Clamp(paneHeight - STACKED_LIST_MIN_HEIGHT - LayoutMetrics.SECTION_SPACING, needed, wanted);
                jobsHeight = paneHeight - inspectorHeight - LayoutMetrics.SECTION_SPACING;
                rulesHeight = jobsHeight;
            }

            var jobsCard = new Card(ModText.BC_UI_Jobs.GetString(), jobsWidth, jobsHeight);
            var jobsContentWidth = Card.ContentWidth(jobsWidth);
            var jobsListHeight = Card.ContentHeight(jobsHeight)
                - LayoutMetrics.LABEL_HEIGHT
                - LayoutMetrics.CONTROL_HEIGHT
                - LayoutMetrics.BUTTON_HEIGHT
                - LayoutMetrics.ROW_SPACING * 3f;

            _jobList = ControlFactory.CreateList<PaintJob>(jobsContentWidth, Math.Max(jobsListHeight, LayoutMetrics.CONTROL_HEIGHT));
            _jobNameField = ControlFactory.CreateTextField(jobsContentWidth);

            var newJobButton = ControlFactory.CreateButton(ModText.BC_UI_New.GetString());
            _copyJobButton = ControlFactory.CreateButton(ModText.BC_UI_Copy.GetString());
            _removeJobButton = ControlFactory.CreateButton(ModText.BC_UI_Remove.GetString(), role: ButtonRole.Danger);
            _shareJobButton = ControlFactory.CreateButton(ModText.BC_Share_Share.GetString());
            _inboxButton = ControlFactory.CreateButton(ModText.BC_Share_Inbox.GetString());

            jobsCard.Content.Add(_jobList, 0f);
            jobsCard.Content.Add(ControlFactory.CreateCaption(ModText.BC_UI_JobName.GetString(), jobsContentWidth), 0f);
            jobsCard.Content.Add(_jobNameField, 0f);
            jobsCard.Content.Add(ControlFactory.CreateButtonRow(jobsContentWidth, newJobButton, _copyJobButton, _removeJobButton), 0f);
            jobsCard.Content.Add(ControlFactory.CreateButtonRow(jobsContentWidth, _shareJobButton, _inboxButton), 0f);

            _rulesCard = new Card(ModText.BC_UI_Rules.GetString(), rulesWidth, rulesHeight);
            var rulesContentWidth = Card.ContentWidth(rulesWidth);
            var rulesListHeight = Card.ContentHeight(rulesHeight)
                - LayoutMetrics.LABEL_HEIGHT
                - LayoutMetrics.CONTROL_HEIGHT
                - LayoutMetrics.BUTTON_HEIGHT * 2f
                - LayoutMetrics.ROW_SPACING * 4f;

            _ruleList = ControlFactory.CreateList<PaintRule>(rulesContentWidth, Math.Max(rulesListHeight, LayoutMetrics.CONTROL_HEIGHT));
            _ruleNameField = ControlFactory.CreateTextField(rulesContentWidth);

            _addRuleButton = ControlFactory.CreateButton(ModText.BC_UI_AddRule.GetString());
            _removeRuleButton = ControlFactory.CreateButton(ModText.BC_UI_RemoveRule.GetString(), role: ButtonRole.Danger);
            _moveRuleUpButton = ControlFactory.CreateButton(ModText.BC_UI_MoveUp.GetString());
            _moveRuleDownButton = ControlFactory.CreateButton(ModText.BC_UI_MoveDown.GetString());

            _rulesCard.Content.Add(_ruleList, 0f);
            _rulesCard.Content.Add(ControlFactory.CreateCaption(ModText.BC_UI_RuleName.GetString(), rulesContentWidth), 0f);
            _rulesCard.Content.Add(_ruleNameField, 0f);
            _rulesCard.Content.Add(ControlFactory.CreateButtonRow(rulesContentWidth, _addRuleButton, _removeRuleButton), 0f);
            _rulesCard.Content.Add(ControlFactory.CreateButtonRow(rulesContentWidth, _moveRuleUpButton, _moveRuleDownButton), 0f);

            _inspectorCard = new Card(ModText.BC_UI_RuleDetails.GetString(), inspectorWidth, inspectorHeight);
            var inspectorContentWidth = Card.ContentWidth(inspectorWidth);

            _conditionsTabButton = ControlFactory.CreateButton(ModText.BC_UI_TabConditions.GetString());
            _paintTabButton = ControlFactory.CreateButton(ModText.BC_UI_TabPaint.GetString());

            var tabRow = ControlFactory.CreateButtonRow(inspectorContentWidth, _conditionsTabButton, _paintTabButton);

            var tabHeight = Card.ContentHeight(inspectorHeight) - LayoutMetrics.BUTTON_HEIGHT - LayoutMetrics.ROW_SPACING;

            _conditionTree = ControlFactory.CreateList<PaintRuleNode>(inspectorContentWidth, tabHeight - LayoutMetrics.BUTTON_HEIGHT - LayoutMetrics.ROW_SPACING);
            _editConditionsButton = ControlFactory.CreateButton(ModText.BC_UI_EditConditions.GetString());

            _conditionsView = new HudChain(true) {
                CollectionContainer = { _conditionTree, _editConditionsButton },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = inspectorContentWidth,
                Height = tabHeight,
            };

            _applyColorCheckbox = ControlFactory.CreateCheckbox();
            _applySkinCheckbox = ControlFactory.CreateCheckbox();

            _sourceTypeDropdown = ControlFactory.CreateDropdown<PaintSourceType>(ControlFactory.ControlWidth(inspectorContentWidth));
            _sourceTypeDropdown.Add(ModText.BC_UI_SourceType_Solid.GetString(), PaintSourceType.Solid);
            _sourceTypeDropdown.Add(ModText.BC_UI_SourceType_Gradient.GetString(), PaintSourceType.Gradient);
            _sourceTypeDropdown.Add(ModText.BC_UI_SourceType_Camo.GetString(), PaintSourceType.Camo);
            _sourceTypeDropdown.Add(ModText.BC_UI_SourceType_Scatter.GetString(), PaintSourceType.Scatter);
            _sourceTypeDropdown.Add(ModText.BC_UI_SourceType_Pattern.GetString(), PaintSourceType.Pattern);

            var channelRow = new HudChain(false) {
                CollectionContainer = {
                    ControlFactory.CreateCheckboxRow(_applyColorCheckbox, ModText.BC_UI_ApplyColor.GetString(), inspectorContentWidth * .5f),
                    ControlFactory.CreateCheckboxRow(_applySkinCheckbox, ModText.BC_UI_ApplySkin.GetString(), inspectorContentWidth * .5f)
                },
                Spacing = 0f,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = inspectorContentWidth,
                Height = LayoutMetrics.CHECKBOX_SIZE,
            };

            _colorPicker = new ColorPickerHSV2() {
                Width = inspectorContentWidth,
                Height = LayoutMetrics.COLOR_PICKER_HEIGHT,
                Name = ModText.BC_UI_TargetColor.GetString(),
            };

            _palette = new ColorPaletteSelector() { Width = inspectorContentWidth };
            _palette.ColorPicked += OnPaletteColorPicked;

            _skinDropdown = DefinitionCatalog.CreateSkinDropdown(LayoutMetrics.CONTROL_HEIGHT);
            _skinDropdown.DimAlignment = DimAlignments.None;
            _skinDropdown.Width = ControlFactory.ControlWidth(inspectorContentWidth);

            var showPalette = paintChrome + stackHeight + paletteCost <= tabHeight;

            _solidSection = new HudChain(true) {
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = inspectorContentWidth,
                Height = stackHeight + (showPalette ? paletteCost : 0f),
            };

            _solidSection.Add(_colorPicker, 0f);

            if (showPalette) {
                _solidSection.Add(ControlFactory.CreateCaption(ModText.BC_UI_PickFromPalette.GetString(), inspectorContentWidth), 0f);
                _solidSection.Add(_palette, 0f);
            }

            _solidSection.Add(ControlFactory.CreateControlRow(ModText.BC_UI_TargetSkin.GetString(), _skinDropdown, inspectorContentWidth), 0f);

            _sourceSummaryLabel = ControlFactory.CreateLabel(string.Empty, inspectorContentWidth, LayoutMetrics.LABEL_HEIGHT * 2f);
            _sourceSummaryLabel.BuilderMode = TextBuilderModes.Wrapped;
            _editSourceButton = ControlFactory.CreateButton(ModText.BC_UI_EditSource.GetString());

            _sourceSection = new HudChain(true) {
                CollectionContainer = { _sourceSummaryLabel, _editSourceButton },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = inspectorContentWidth,
                Height = _sourceSummaryLabel.Height + LayoutMetrics.BUTTON_HEIGHT + LayoutMetrics.ROW_SPACING,
            };

            _paintView = new HudChain(true) {
                CollectionContainer = {
                    channelRow,
                    ControlFactory.CreateControlRow(ModText.BC_UI_SourceType.GetString(), _sourceTypeDropdown, inspectorContentWidth),
                    _solidSection,
                    _sourceSection
                },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = inspectorContentWidth,
                Height = tabHeight,
            };

            _inspectorCard.Content.Add(tabRow, 0f);
            _inspectorCard.Content.Add(_conditionsView, 0f);
            _inspectorCard.Content.Add(_paintView, 0f);

            HudChain panes;

            if (wide) {
                panes = ControlFactory.CreateRow(width, paneHeight, LayoutMetrics.SECTION_SPACING);
                panes.Add(jobsCard, 0f);
                panes.Add(_rulesCard, 0f);
            } else {
                var listRow = ControlFactory.CreateRow(width, jobsHeight, LayoutMetrics.SECTION_SPACING);
                listRow.Add(jobsCard, 0f);
                listRow.Add(_rulesCard, 0f);

                panes = ControlFactory.CreateColumn(width, paneHeight, LayoutMetrics.SECTION_SPACING);
                panes.Add(listRow, 0f);
            }

            panes.Add(_inspectorCard, 0f);

            _includeSubgridsCheckbox = ControlFactory.CreateCheckbox();
            _includeProjectedCheckbox = ControlFactory.CreateCheckbox();
            _includePreviewCheckbox = ControlFactory.CreateCheckbox();

            var optionWidth = width / 3f;
            var optionsRow = new HudChain(false) {
                CollectionContainer = {
                    ControlFactory.CreateCheckboxRow(_includeSubgridsCheckbox, ModText.BC_UI_Option_IncludeSubgrids.GetString(), optionWidth),
                    ControlFactory.CreateCheckboxRow(_includeProjectedCheckbox, ModText.BC_UI_Option_IncludeProjected.GetString(), optionWidth),
                    ControlFactory.CreateCheckboxRow(_includePreviewCheckbox, ModText.BC_UI_Option_IncludePreview.GetString(), optionWidth)
                },
                Spacing = 0f,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = LayoutMetrics.CHECKBOX_SIZE,
            };

            var options = ControlFactory.CreateColumn(width, optionsHeight, LayoutMetrics.TIGHT_SPACING);
            options.Add(ControlFactory.CreateHeading(ModText.BC_UI_JobOptions.GetString(), width), 0f);
            options.Add(optionsRow, 0f);

            var layout = new HudChain(true, this) {
                ParentAlignment = ParentAlignments.Inner,
                Spacing = LayoutMetrics.SECTION_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = height,
            };

            layout.Add(toolbar, 0f);
            layout.Add(panes, 0f);
            layout.Add(options, 0f);

            _jobList.ValueChanged += OnJobSelected;
            _ruleList.ValueChanged += OnRuleSelected;

            newJobButton.MouseInput.LeftClicked += OnNewJob;
            _copyJobButton.MouseInput.LeftClicked += OnCopyJob;
            _removeJobButton.MouseInput.LeftClicked += OnRemoveJob;

            _addRuleButton.MouseInput.LeftClicked += OnAddRule;
            _removeRuleButton.MouseInput.LeftClicked += OnRemoveRule;
            _moveRuleUpButton.MouseInput.LeftClicked += (sender, args) => MoveRule(-1);
            _moveRuleDownButton.MouseInput.LeftClicked += (sender, args) => MoveRule(1);

            _conditionsTabButton.MouseInput.LeftClicked += (sender, args) => ShowTab(true);
            _paintTabButton.MouseInput.LeftClicked += (sender, args) => ShowTab(false);
            _editConditionsButton.MouseInput.LeftClicked += OnEditConditions;
            _editSourceButton.MouseInput.LeftClicked += OnEditSource;

            _sourceTypeDropdown.ValueChanged += OnSourceTypeChanged;
            _colorPicker.ColorChanged += OnColorPickerChanged;
            _applyColorCheckbox.MouseInput.LeftClicked += (sender, args) => WriteRule();
            _applySkinCheckbox.MouseInput.LeftClicked += (sender, args) => WriteRule();
            _skinDropdown.ValueChanged += (sender, args) => WriteRule();

            _includeSubgridsCheckbox.MouseInput.LeftClicked += (sender, args) => WriteOptions();
            _includeProjectedCheckbox.MouseInput.LeftClicked += (sender, args) => WriteOptions();
            _includePreviewCheckbox.MouseInput.LeftClicked += (sender, args) => WriteOptions();

            _applyButton.MouseInput.LeftClicked += OnApply;
            _undoButton.MouseInput.LeftClicked += OnUndo;
            _shareJobButton.MouseInput.LeftClicked += OnShareJob;
            _inboxButton.MouseInput.LeftClicked += OnOpenInbox;

            ShowTab(true);
            Refresh();
        }

        private static PaintJobService Service {
            get { return Mod.Static?.PaintJobService; }
        }

        public override void Refresh() {
            Refresh(null);
        }

        /// <summary>
        /// Rebuilds the job list, landing on the given job or keeping the current selection.
        /// </summary>
        public void Refresh(PaintJob jobToSelect) {
            RefreshHotkeyHint();
            RefreshShares();

            var jobs = Service != null ? Service.GetJobs() : null;
            var selection = jobToSelect ?? _loadedJob;

            _suppressWrites = true;
            _jobList.ClearEntries();

            if (jobs != null) {
                foreach (var job in PaintJobService.InNameOrder(jobs)) {
                    _jobList.Add(job.Name, job);
                }
            }

            _suppressWrites = false;

            if (_jobList.Count == 0) {
                _loadedJob = null;
                _loadedRule = null;
                RefreshRules();
                UpdateButtonState();
                return;
            }

            var index = IndexOfJob(selection);
            _jobList.SetSelectionAt(index >= 0 ? index : 0);
        }

        /// <summary>
        /// Writes the hint from the binds as they are set right now, which a rebind changes.
        /// </summary>
        private void RefreshHotkeyHint() {
            _hotkeyHint.Text = ModText.BC_UI_HotkeyHint.GetString(
                PaintJobInput.DescribeBind(PaintJobInput.APPLY_BIND),
                PaintJobInput.DescribeBind(PaintJobInput.UNDO_BIND),
                PaintJobInput.DescribeBind(PaintJobInput.PREVIOUS_JOB_BIND),
                PaintJobInput.DescribeBind(PaintJobInput.NEXT_JOB_BIND));
        }

        public override void Commit() {
            CommitNames();
        }

        protected override void HandleInput(Vector2 cursorPos) {
            base.HandleInput(cursorPos);

            if (ActiveDialog != null) {
                return;
            }

            if (SharedBinds.Enter.IsNewPressed || SharedBinds.LeftButton.IsNewPressed) {
                CommitNames();
            }
        }

        private int IndexOfJob(PaintJob job) {
            if (job == null) {
                return -1;
            }

            for (var i = 0; i < _jobList.EntryList.Count; i++) {
                if (_jobList.EntryList[i].AssocMember != null && _jobList.EntryList[i].AssocMember.Id == job.Id) {
                    return i;
                }
            }

            return -1;
        }

        private void OnJobSelected(object sender, EventArgs e) {
            _loadedJob = _jobList.Value != null ? _jobList.Value.AssocMember : null;
            _loadedRule = null;

            Service?.SetActiveJob(_loadedJob);

            _suppressWrites = true;

            if (_loadedJob != null) {
                _loadedJob.EnsureRules();
                _jobNameField.Text = _loadedJob.Name;

                var options = _loadedJob.Options ?? (_loadedJob.Options = new PaintJobOptions());
                _includeSubgridsCheckbox.Value = options.IncludeSubgrids;
                _includeProjectedCheckbox.Value = options.IncludeProjectedGrids;
                _includePreviewCheckbox.Value = options.IncludePreviewGrids;
            } else {
                _jobNameField.Text = string.Empty;
            }

            _suppressWrites = false;

            RefreshRules();
            UpdateButtonState();
        }

        private void OnNewJob(object sender, EventArgs e) {
            var job = Service?.CreateJob();
            if (job == null) {
                return;
            }

            Refresh(job);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnCopyJob(object sender, EventArgs e) {
            if (_loadedJob == null || Service == null) {
                return;
            }

            var copy = _loadedJob.Clone();
            copy.Id = Guid.NewGuid();
            copy.Name = Service.UniqueJobName(_loadedJob.Name);

            Service.SaveJob(copy);
            Refresh(copy);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        /// <summary>
        /// Puts the number of waiting shares on the inbox button.
        /// </summary>
        public override void RefreshShares() {
            var count = Mod.Static?.Inbox?.Count ?? 0;

            _inboxButton.Text = count > 0
                ? ModText.BC_Share_InboxWithCount.GetString(count)
                : ModText.BC_Share_Inbox.GetString();
        }

        private void OnShareJob(object sender, EventArgs e) {
            if (ActiveDialog != null || _loadedJob == null) {
                return;
            }

            var job = _loadedJob.Clone();
            var dialog = new ShareDialog(ModText.BC_Share_ShareTitle.GetString(job.Name));

            dialog.Confirmed += recipient => ShareService.Share(new SharePacket { Kind = ShareKind.PaintJob, PaintJob = job }, recipient);

            OpenDialog(dialog);
        }

        private void OnOpenInbox(object sender, EventArgs e) {
            if (ActiveDialog != null) {
                return;
            }

            var inbox = Mod.Static?.Inbox;

            if (inbox == null) {
                return;
            }

            var dialog = new InboxDialog(inbox);

            dialog.Closed += (sender2, args) => Refresh();

            OpenDialog(dialog);
        }

        private void OnRemoveJob(object sender, EventArgs e) {
            if (_loadedJob == null || Service == null) {
                return;
            }

            var job = _loadedJob;
            var dialog = new ConfirmDialog(ModText.BC_UI_Remove.GetString(), ModText.BC_UI_Confirm_RemoveJob.GetString(job.Name));

            dialog.Confirmed += (s, args) => {
                Service.RemoveJob(job);
                _loadedJob = null;

                Refresh();
            };

            OpenDialog(dialog);
        }

        private void RefreshRules(PaintRule ruleToSelect = null) {
            var selection = ruleToSelect ?? _loadedRule;

            _suppressWrites = true;
            _ruleList.ClearEntries();

            if (_loadedJob != null) {
                _rulesCard.Title = ModText.BC_UI_RulesOf.GetString(_loadedJob.Name);

                for (var i = 0; i < _loadedJob.Rules.Count; i++) {
                    _ruleList.Add(string.Format("{0}. {1}", i + 1, _loadedJob.Rules[i].Name), _loadedJob.Rules[i]);
                }
            } else {
                _rulesCard.Title = ModText.BC_UI_Rules.GetString();
            }

            _suppressWrites = false;

            if (_ruleList.Count == 0) {
                _loadedRule = null;
                LoadRule();
                return;
            }

            var index = selection != null ? _loadedJob.Rules.IndexOf(selection) : 0;
            _ruleList.SetSelectionAt(index >= 0 && index < _ruleList.Count ? index : 0);
        }

        private void OnRuleSelected(object sender, EventArgs e) {
            _loadedRule = _ruleList.Value != null ? _ruleList.Value.AssocMember : null;

            LoadRule();
            UpdateButtonState();
        }

        private void OnAddRule(object sender, EventArgs e) {
            if (_loadedJob == null) {
                return;
            }

            var index = _loadedJob.Rules.Count + 1;
            var name = ModText.BC_UI_DefaultRuleName.GetString(index);

            while (_loadedJob.Rules.Exists(rule => string.Equals(rule.Name, name, StringComparison.InvariantCultureIgnoreCase))) {
                index++;
                name = ModText.BC_UI_DefaultRuleName.GetString(index);
            }

            var newRule = PaintRule.CreateDefault(name);
            _loadedJob.Rules.Add(newRule);

            Service?.Save();
            RefreshRules(newRule);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnRemoveRule(object sender, EventArgs e) {
            if (_loadedJob == null || _loadedRule == null) {
                return;
            }

            if (_loadedJob.Rules.Count <= 1) {
                HudSoundUtils.PlaySound("HudLockingLost");
                return;
            }

            _loadedJob.Rules.Remove(_loadedRule);
            _loadedRule = null;

            Service?.Save();
            RefreshRules();
            HudSoundUtils.PlaySound("HudLockingLost");
        }

        /// <summary>
        /// Rules are tested top down, so their order is what decides which one claims a block.
        /// </summary>
        private void MoveRule(int offset) {
            if (_loadedJob == null || _loadedRule == null) {
                return;
            }

            var index = _loadedJob.Rules.IndexOf(_loadedRule);
            var target = index + offset;

            if (index < 0 || target < 0 || target >= _loadedJob.Rules.Count) {
                return;
            }

            _loadedJob.Rules.RemoveAt(index);
            _loadedJob.Rules.Insert(target, _loadedRule);

            Service?.Save();
            RefreshRules(_loadedRule);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void ShowTab(bool conditions) {
            _conditionsView.Visible = conditions;
            _paintView.Visible = !conditions;

            _conditionsTabButton.InputEnabled = !conditions;
            _paintTabButton.InputEnabled = conditions;
        }

        private void LoadRule() {
            _suppressWrites = true;

            if (_loadedRule == null) {
                _inspectorCard.Title = ModText.BC_UI_RuleDetails.GetString();
                _ruleNameField.Text = string.Empty;
                _conditionTree.ClearEntries();
                _sourceSummaryLabel.Text = string.Empty;
                _suppressWrites = false;

                return;
            }

            _inspectorCard.Title = ModText.BC_UI_RuleDetailsOf.GetString(_loadedRule.Name);
            _ruleNameField.Text = _loadedRule.Name;

            if (_loadedRule.Action == null) {
                _loadedRule.Action = new PaintRuleAction();
            }

            _applyColorCheckbox.Value = _loadedRule.Action.ApplyColor;
            _applySkinCheckbox.Value = _loadedRule.Action.ApplySkin;

            _colorPicker.Color = _loadedRule.Action.TargetHsv;

            var skinIndex = DefinitionCatalog.IndexOfSkin(_loadedRule.Action.TargetSkinId);
            _skinDropdown.SetSelectionAt(skinIndex >= 0 ? skinIndex : 0);

            _sourceTypeDropdown.SetSelection(_loadedRule.Action.SourceType);

            RefreshConditionTree();
            UpdateSourceViews();

            _suppressWrites = false;
        }

        private void RefreshConditionTree() {
            _conditionTree.ClearEntries();

            if (_loadedRule == null || _loadedRule.ConditionGroup == null) {
                return;
            }

            foreach (var node in PaintRulePath.Flatten(_loadedRule.ConditionGroup)) {
                var indent = new string(' ', node.Depth * 3);
                _conditionTree.Add(string.Format("{0}[{1}] {2}", indent, node.Path, PaintJobReport.DescribeNode(node)), node);
            }
        }

        private void UpdateSourceViews() {
            var isSolid = GetSelectedSourceType() == PaintSourceType.Solid;

            _solidSection.Visible = isSolid;
            _sourceSection.Visible = !isSolid;

            _sourceSummaryLabel.Text = _loadedRule != null && _loadedRule.Action != null
                ? PaintSourceText.Describe(_loadedRule.Action.Source)
                : string.Empty;
        }

        private PaintSourceType GetSelectedSourceType() {
            return _sourceTypeDropdown.Value != null ? _sourceTypeDropdown.Value.AssocMember : PaintSourceType.Solid;
        }

        private void OnSourceTypeChanged(object sender, EventArgs e) {
            WriteRule();
            UpdateSourceViews();
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnPaletteColorPicked(ColorMask mask) {
            _colorPicker.Color = mask;
            WriteRule();
        }

        /// <summary>
        /// The picker also fires this when a rule is loaded into it, so the rule is only written when the color really moved.
        /// </summary>
        private void OnColorPickerChanged(object sender, EventArgs e) {
            if (_suppressWrites || _loadedRule == null || _loadedRule.Action == null) {
                return;
            }

            var color = _colorPicker.Color;
            var target = _loadedRule.Action.TargetHsv;

            if (target.H == color.H && target.S == color.S && target.V == color.V) {
                return;
            }

            WriteRule();
        }

        /// <summary>
        /// Copies the editor onto the selected rule.
        /// </summary>
        private void WriteRule() {
            if (_suppressWrites || _loadedRule == null) {
                return;
            }

            if (_loadedRule.Action == null) {
                _loadedRule.Action = new PaintRuleAction();
            }

            var action = _loadedRule.Action;

            action.ApplyColor = _applyColorCheckbox.Value;
            action.ApplySkin = _applySkinCheckbox.Value;

            action.TargetHsv = _colorPicker.Color;

            var skin = _skinDropdown.Value != null ? _skinDropdown.Value.AssocMember : null;
            action.TargetSkinId = skin != null ? skin.SkinId : string.Empty;

            var type = GetSelectedSourceType();

            if (type == PaintSourceType.Solid) {
                if (action.Source != null) {
                    action.Source.Type = PaintSourceType.Solid;
                }
            } else {
                if (action.Source == null) {
                    action.Source = new PaintColorSource();
                }

                action.Source.Type = type;
                action.Source.EnsureEntries(action.TargetColor);
            }

            Service?.Save();
        }

        private void WriteOptions() {
            if (_suppressWrites || _loadedJob == null) {
                return;
            }

            var options = _loadedJob.Options ?? (_loadedJob.Options = new PaintJobOptions());

            options.IncludeSubgrids = _includeSubgridsCheckbox.Value;
            options.IncludeProjectedGrids = _includeProjectedCheckbox.Value;
            options.IncludePreviewGrids = _includePreviewCheckbox.Value;

            Service?.Save();
        }

        /// <summary>
        /// Writes the edited job and rule names back.
        /// </summary>
        private void CommitNames() {
            if (_loadedJob != null) {
                var jobName = _jobNameField.Text.ToString().Trim();

                if (!string.IsNullOrEmpty(jobName) && jobName != _loadedJob.Name) {
                    _loadedJob.Name = jobName;
                    Service?.Save();
                    Refresh(_loadedJob);
                }
            }

            if (_loadedRule != null) {
                var ruleName = _ruleNameField.Text.ToString().Trim();

                if (!string.IsNullOrEmpty(ruleName) && ruleName != _loadedRule.Name) {
                    _loadedRule.Name = ruleName;
                    Service?.Save();
                    RefreshRules(_loadedRule);
                }
            }
        }

        private void OnApply(object sender, EventArgs e) {
            if (_loadedJob == null) {
                return;
            }

            Service?.ApplyJobToSelection(_loadedJob);
            UpdateButtonState();
            HudSoundUtils.PlaySound("HudBleep");
        }

        private void OnUndo(object sender, EventArgs e) {
            Service?.UndoPaintJob();
            UpdateButtonState();
            HudSoundUtils.PlaySound("HudLockingLost");
        }

        private void OnEditConditions(object sender, EventArgs e) {
            if (_loadedRule == null || ActiveDialog != null) {
                return;
            }

            var dialog = new PaintRuleConditionGroupDialog(_loadedRule);
            dialog.Saved += (s, args) => {
                Service?.Save();
                RefreshConditionTree();
            };

            OpenDialog(dialog);
        }

        private void OnEditSource(object sender, EventArgs e) {
            if (_loadedRule == null || ActiveDialog != null) {
                return;
            }

            WriteRule();

            var source = _loadedRule.Action.Source;
            if (source == null || source.Type == PaintSourceType.Solid) {
                return;
            }

            var dialog = new PaintSourceDialog(source, _loadedRule.Action.TargetColor);
            dialog.Saved += (s, args) => {
                Service?.Save();
                UpdateSourceViews();
            };

            OpenDialog(dialog);
        }

        protected override void DialogClosed() {
            RefreshConditionTree();
            UpdateSourceViews();
        }

        private void UpdateButtonState() {
            var hasJob = _loadedJob != null;
            var hasRule = _loadedRule != null;

            _copyJobButton.InputEnabled = hasJob;
            _removeJobButton.InputEnabled = hasJob;
            _shareJobButton.InputEnabled = hasJob;
            _applyButton.InputEnabled = hasJob;
            _addRuleButton.InputEnabled = hasJob;

            _removeRuleButton.InputEnabled = hasRule && _loadedJob != null && _loadedJob.Rules.Count > 1;
            _moveRuleUpButton.InputEnabled = hasRule;
            _moveRuleDownButton.InputEnabled = hasRule;
            _editConditionsButton.InputEnabled = hasRule;
            _editSourceButton.InputEnabled = hasRule;

            _undoButton.InputEnabled = (Service?.UndoCount ?? 0) > 0;
        }
    }
}
