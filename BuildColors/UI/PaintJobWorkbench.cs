using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Services;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// The paint job workbench: jobs, the rules of the selected job and the detail of the selected rule, all
    /// on one screen.
    /// <para>
    /// It replaces a stack of four nested dialogs. Everything is edited in place and written straight back,
    /// so there is nothing to commit and no working copy to lose - which is what lets the three panes stay
    /// live next to each other instead of taking turns.
    /// </para>
    /// </summary>
    public class PaintJobWorkbench : DialogBase {
        private const float BUTTON_HEIGHT = LayoutMetrics.BUTTON_HEIGHT;
        private const float JOBS_PANE_WIDTH = 300f;
        private const float MAX_HEIGHT = 860f;
        /// <summary>
        /// The inspector takes whatever the two lists beside it leave, so the width of the window is what
        /// decides how wide it gets. Wider than this and the inspector is mostly empty space: it holds one
        /// control per row and a condition tree, neither of which reads better for being stretched.
        /// </summary>
        private const float MAX_WIDTH = 1200f;

        /// <summary>
        /// Gap left at the sides of the screen on small displays.
        /// </summary>
        private const float SCREEN_MARGIN = 60f;

        private const float MIN_HEIGHT = 700f;
        private const float MIN_WIDTH = 1100f;
        private const float PANE_SPACING = 16f;

        /// <summary>
        /// Slack left at the bottom of every pane. A chain that is filled to the last pixel overlaps its own
        /// members the moment one of them measures larger than expected, so none of them is filled exactly.
        /// </summary>
        private const float PANE_SLACK = 12f;
        private const float RULES_PANE_WIDTH = 340f;

        private readonly ListBox<PaintJob> _jobList;
        private readonly TextField _jobNameField;
        private readonly BorderedButton _removeJobButton;
        private readonly BorderedButton _copyJobButton;

        private readonly Label _rulesLabel;
        private readonly ListBox<PaintRule> _ruleList;
        private readonly TextField _ruleNameField;
        private readonly BorderedButton _addRuleButton;
        private readonly BorderedButton _removeRuleButton;
        private readonly BorderedButton _moveRuleUpButton;
        private readonly BorderedButton _moveRuleDownButton;

        private readonly Label _inspectorLabel;
        private readonly BorderedButton _conditionsTabButton;
        private readonly BorderedButton _paintTabButton;

        private readonly HudChain _conditionsView;
        private readonly ListBox<PaintRuleNode> _conditionTree;
        private readonly BorderedButton _editConditionsButton;

        private readonly HudChain _paintView;
        private readonly BorderedCheckBox _applyColorCheckbox;
        private readonly BorderedCheckBox _applySkinCheckbox;
        private readonly Dropdown<PaintSourceType> _sourceTypeDropdown;
        private readonly HudChain _solidSection;
        private readonly ColorPickerHSV _colorPicker;
        private readonly ColorPaletteSelector _palette;
        private readonly Dropdown<SkinListEntry, DefinitionCatalog.SkinOption> _skinDropdown;
        private readonly HudChain _sourceSection;
        private readonly Label _sourceSummaryLabel;
        private readonly BorderedButton _editSourceButton;

        private readonly BorderedCheckBox _includeSubgridsCheckbox;
        private readonly BorderedCheckBox _includeProjectedCheckbox;
        private readonly BorderedCheckBox _includePreviewCheckbox;
        private readonly BorderedCheckBox _respectOwnershipCheckbox;

        private readonly BorderedButton _applyButton;
        private readonly BorderedButton _undoButton;
        private readonly Label _statusLabel;

        private DialogBase _activeDialog;
        private PaintJob _loadedJob;
        private PaintRule _loadedRule;
        private bool _suppressWrites;

        public PaintJobWorkbench(HudParentBase parent = null) : base(parent) {
            var screen = DialogSafeArea.ScreenSize;
            var width = MathHelper.Clamp(screen.X - SCREEN_MARGIN * 2f, MIN_WIDTH, MAX_WIDTH);
            var height = GetSafeHeight(MAX_HEIGHT, MIN_HEIGHT);

            Size = new Vector2(width, height);
            HeaderText = ModText.BC_UI_WorkbenchTitle.GetString();

            var contentWidth = width - Padding.X - LayoutMetrics.CONTENT_PADDING_X;
            var contentHeight = height - Padding.Y - HEADER_HEIGHT - LayoutMetrics.CONTENT_PADDING_Y;

            // Four rows: the toolbar, the panes, the job options and the status line. Only the panes grow,
            // so the height every pane gets is what is left once the other three have been taken off.
            var paneHeight = contentHeight
                - BUTTON_HEIGHT
                - LayoutMetrics.CHECKBOX_SIZE
                - LayoutMetrics.LABEL_HEIGHT
                - LayoutMetrics.SECTION_SPACING * 3f;

            var inspectorWidth = contentWidth - JOBS_PANE_WIDTH - RULES_PANE_WIDTH - PANE_SPACING * 2f;

            // ---- toolbar ----
            _applyButton = CreateButton(ModText.BC_UI_Apply.GetString(), 160f);
            _undoButton = CreateButton(ModText.BC_UI_Undo.GetString(), 160f);

            var toolbar = new HudChain(false) {
                CollectionContainer = { _applyButton, _undoButton },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.AlignMembersEnd,
                Width = contentWidth,
                Height = BUTTON_HEIGHT,
            };

            // ---- jobs pane ----
            _jobList = CreateList<PaintJob>(JOBS_PANE_WIDTH, ListHeight(paneHeight, 2));
            _jobNameField = CreateTextField(JOBS_PANE_WIDTH);

            var newJobButton = CreateButton(ModText.BC_UI_New.GetString());
            _copyJobButton = CreateButton(ModText.BC_UI_Copy.GetString());
            _removeJobButton = CreateButton(ModText.BC_UI_Remove.GetString());

            var jobsPane = CreatePane(JOBS_PANE_WIDTH, paneHeight);
            jobsPane.Add(CreateLabel(ModText.BC_UI_Jobs.GetString(), JOBS_PANE_WIDTH), 0f);
            jobsPane.Add(_jobList, 0f);
            jobsPane.Add(_jobNameField, 0f);
            jobsPane.Add(CreateButtonRow(JOBS_PANE_WIDTH, newJobButton, _copyJobButton, _removeJobButton), 0f);

            // ---- rules pane ----
            _rulesLabel = CreateLabel(string.Empty, RULES_PANE_WIDTH);
            _ruleList = CreateList<PaintRule>(RULES_PANE_WIDTH, ListHeight(paneHeight, 3));
            _ruleNameField = CreateTextField(RULES_PANE_WIDTH);

            _addRuleButton = CreateButton(ModText.BC_UI_AddRule.GetString());
            _removeRuleButton = CreateButton(ModText.BC_UI_RemoveRule.GetString());
            _moveRuleUpButton = CreateButton(ModText.BC_UI_MoveUp.GetString());
            _moveRuleDownButton = CreateButton(ModText.BC_UI_MoveDown.GetString());

            var rulesPane = CreatePane(RULES_PANE_WIDTH, paneHeight);
            rulesPane.Add(_rulesLabel, 0f);
            rulesPane.Add(_ruleList, 0f);
            rulesPane.Add(_ruleNameField, 0f);
            rulesPane.Add(CreateButtonRow(RULES_PANE_WIDTH, _addRuleButton, _removeRuleButton), 0f);
            rulesPane.Add(CreateButtonRow(RULES_PANE_WIDTH, _moveRuleUpButton, _moveRuleDownButton), 0f);

            // ---- inspector ----
            _inspectorLabel = CreateLabel(string.Empty, inspectorWidth);

            _conditionsTabButton = CreateButton(ModText.BC_UI_TabConditions.GetString());
            _paintTabButton = CreateButton(ModText.BC_UI_TabPaint.GetString());

            var tabRow = CreateButtonRow(inspectorWidth, _conditionsTabButton, _paintTabButton);

            // The two tabs occupy the same slot, so the height one of them may take is the height both get.
            var tabHeight = paneHeight - LayoutMetrics.LABEL_HEIGHT - BUTTON_HEIGHT - LayoutMetrics.SECTION_SPACING * 2f - PANE_SLACK;

            _conditionTree = CreateList<PaintRuleNode>(inspectorWidth, tabHeight - BUTTON_HEIGHT - LayoutMetrics.SECTION_SPACING);
            _editConditionsButton = CreateButton(ModText.BC_UI_EditConditions.GetString());

            _conditionsView = new HudChain(true) {
                CollectionContainer = { _conditionTree, _editConditionsButton },
                Spacing = LayoutMetrics.SECTION_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = inspectorWidth,
                Height = tabHeight,
            };

            _applyColorCheckbox = new BorderedCheckBox();
            _applySkinCheckbox = new BorderedCheckBox();

            _sourceTypeDropdown = new Dropdown<PaintSourceType>() { Width = inspectorWidth, Height = LayoutMetrics.CONTROL_HEIGHT };
            _sourceTypeDropdown.Add(ModText.BC_UI_SourceType_Solid.GetString(), PaintSourceType.Solid);
            _sourceTypeDropdown.Add(ModText.BC_UI_SourceType_Gradient.GetString(), PaintSourceType.Gradient);
            _sourceTypeDropdown.Add(ModText.BC_UI_SourceType_Camo.GetString(), PaintSourceType.Camo);
            _sourceTypeDropdown.Add(ModText.BC_UI_SourceType_Scatter.GetString(), PaintSourceType.Scatter);
            _sourceTypeDropdown.Add(ModText.BC_UI_SourceType_Pattern.GetString(), PaintSourceType.Pattern);

            var channelRow = new HudChain(false) {
                CollectionContainer = {
                    CreateCheckboxRow(_applyColorCheckbox, ModText.BC_UI_ApplyColor.GetString(), inspectorWidth * .5f),
                    CreateCheckboxRow(_applySkinCheckbox, ModText.BC_UI_ApplySkin.GetString(), inspectorWidth * .5f)
                },
                Spacing = 0f,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = inspectorWidth,
                Height = LayoutMetrics.CHECKBOX_SIZE,
            };

            _colorPicker = new ColorPickerHSV() {
                Width = inspectorWidth,
                Height = LayoutMetrics.COLOR_PICKER_HEIGHT,
                Name = ModText.BC_UI_TargetColor.GetString(),
            };

            _palette = new ColorPaletteSelector() { Width = inspectorWidth };
            _palette.ColorPicked += OnPaletteColorPicked;

            _skinDropdown = DefinitionCatalog.CreateSkinDropdown(LayoutMetrics.CONTROL_HEIGHT);
            _skinDropdown.DimAlignment = DimAlignments.None;
            _skinDropdown.Width = inspectorWidth;

            // Color above skin. Stacked they cost the sum of their heights rather than the tallest, so on a
            // window too short for all of it the swatches are what gives way - the picker can reach the same
            // colors, and a section that does not fit overlaps itself rather than clipping.
            var stackHeight = LayoutMetrics.COLOR_PICKER_HEIGHT
                + LayoutMetrics.LABEL_HEIGHT
                + LayoutMetrics.CONTROL_HEIGHT
                + LayoutMetrics.ROW_SPACING * 2f;

            var paintChrome = LayoutMetrics.CHECKBOX_SIZE + LayoutMetrics.CONTROL_HEIGHT + LayoutMetrics.SECTION_SPACING * 2f;
            var paletteCost = ColorPaletteSelector.TOTAL_HEIGHT + LayoutMetrics.ROW_SPACING;
            var showPalette = paintChrome + stackHeight + paletteCost <= tabHeight;

            _solidSection = new HudChain(true) {
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = inspectorWidth,
                Height = stackHeight + (showPalette ? paletteCost : 0f),
            };

            _solidSection.Add(_colorPicker, 0f);

            if (showPalette) {
                _solidSection.Add(_palette, 0f);
            }

            _solidSection.Add(CreateLabel(ModText.BC_UI_TargetSkin.GetString(), inspectorWidth), 0f);
            _solidSection.Add(_skinDropdown, 0f);

            _sourceSummaryLabel = CreateLabel(string.Empty, inspectorWidth);
            _editSourceButton = CreateButton(ModText.BC_UI_EditSource.GetString());

            _sourceSection = new HudChain(true) {
                CollectionContainer = { _sourceSummaryLabel, _editSourceButton },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = inspectorWidth,
                Height = LayoutMetrics.LABEL_HEIGHT + BUTTON_HEIGHT + LayoutMetrics.ROW_SPACING,
            };

            _paintView = new HudChain(true) {
                CollectionContainer = { channelRow, _sourceTypeDropdown, _solidSection, _sourceSection },
                Spacing = LayoutMetrics.SECTION_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = inspectorWidth,
                Height = tabHeight,
            };

            var inspectorPane = CreatePane(inspectorWidth, paneHeight);
            inspectorPane.Add(_inspectorLabel, 0f);
            inspectorPane.Add(tabRow, 0f);
            inspectorPane.Add(_conditionsView, 0f);
            inspectorPane.Add(_paintView, 0f);

            var panes = new HudChain(false) {
                CollectionContainer = { jobsPane, rulesPane, inspectorPane },
                Spacing = PANE_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = paneHeight,
            };

            // ---- options ----
            _includeSubgridsCheckbox = new BorderedCheckBox();
            _includeProjectedCheckbox = new BorderedCheckBox();
            _includePreviewCheckbox = new BorderedCheckBox();
            _respectOwnershipCheckbox = new BorderedCheckBox();

            var optionWidth = contentWidth * .25f;
            var optionsRow = new HudChain(false) {
                CollectionContainer = {
                    CreateCheckboxRow(_includeSubgridsCheckbox, ModText.BC_UI_Option_IncludeSubgrids.GetString(), optionWidth),
                    CreateCheckboxRow(_includeProjectedCheckbox, ModText.BC_UI_Option_IncludeProjected.GetString(), optionWidth),
                    CreateCheckboxRow(_includePreviewCheckbox, ModText.BC_UI_Option_IncludePreview.GetString(), optionWidth),
                    CreateCheckboxRow(_respectOwnershipCheckbox, ModText.BC_UI_Option_RespectOwnership.GetString(), optionWidth)
                },
                Spacing = 0f,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LayoutMetrics.CHECKBOX_SIZE,
            };

            // ---- status ----
            _statusLabel = new Label() {
                Text = string.Empty,
                Format = Style.BodyText,
                AutoResize = false,
                Height = LayoutMetrics.LABEL_HEIGHT,
            };

            var statusRow = new HudChain(false) {
                CollectionContainer = { { _statusLabel, 1f } },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LayoutMetrics.LABEL_HEIGHT,
            };

            var layout = new HudChain(true, body) {
                ParentAlignment = ParentAlignments.Inner,
                DimAlignment = DimAlignments.UnpaddedSize,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                CollectionContainer = { toolbar, panes, optionsRow, statusRow },
                Spacing = LayoutMetrics.SECTION_SPACING,
                Padding = new Vector2(LayoutMetrics.CONTENT_PADDING_X, LayoutMetrics.CONTENT_PADDING_Y)
            };

            _jobList.ValueChanged += OnJobSelected;
            _ruleList.ValueChanged += OnRuleSelected;

            newJobButton.MouseInput.LeftClicked += OnNewJob;
            _copyJobButton.MouseInput.LeftClicked += OnCopyJob;
            _removeJobButton.MouseInput.LeftClicked += OnRemoveJob;

            _addRuleButton.MouseInput.LeftClicked += OnAddRule;
            _removeRuleButton.MouseInput.LeftClicked += OnRemoveRule;
            _moveRuleUpButton.MouseInput.LeftClicked += (s, e) => MoveRule(-1);
            _moveRuleDownButton.MouseInput.LeftClicked += (s, e) => MoveRule(1);

            _conditionsTabButton.MouseInput.LeftClicked += (s, e) => ShowTab(true);
            _paintTabButton.MouseInput.LeftClicked += (s, e) => ShowTab(false);
            _editConditionsButton.MouseInput.LeftClicked += OnEditConditions;
            _editSourceButton.MouseInput.LeftClicked += OnEditSource;

            _sourceTypeDropdown.ValueChanged += OnSourceTypeChanged;
            _applyColorCheckbox.MouseInput.LeftClicked += (s, e) => WriteRule();
            _applySkinCheckbox.MouseInput.LeftClicked += (s, e) => WriteRule();
            _skinDropdown.ValueChanged += (s, e) => WriteRule();

            _includeSubgridsCheckbox.MouseInput.LeftClicked += (s, e) => WriteOptions();
            _includeProjectedCheckbox.MouseInput.LeftClicked += (s, e) => WriteOptions();
            _includePreviewCheckbox.MouseInput.LeftClicked += (s, e) => WriteOptions();
            _respectOwnershipCheckbox.MouseInput.LeftClicked += (s, e) => WriteOptions();

            _applyButton.MouseInput.LeftClicked += OnApply;
            _undoButton.MouseInput.LeftClicked += OnUndo;

            ShowTab(true);
            RefreshJobs();
        }

        private static PaintJobService Service {
            get { return Mod.Static?.PaintJobService; }
        }

        /// <summary>
        /// Height a pane gives its list: whatever is left once the label, the name field and the button rows
        /// under it have taken their share.
        /// </summary>
        private static float ListHeight(float paneHeight, int buttonRows) {
            var used = LayoutMetrics.LABEL_HEIGHT
                + LayoutMetrics.CONTROL_HEIGHT
                + BUTTON_HEIGHT * buttonRows
                + LayoutMetrics.SECTION_SPACING * (2f + buttonRows)
                + PANE_SLACK;

            return Math.Max(paneHeight - used, LayoutMetrics.CONTROL_HEIGHT);
        }

        private static HudChain CreatePane(float width, float height) {
            return new HudChain(true) {
                Spacing = LayoutMetrics.SECTION_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = height,
            };
        }

        private static ListBox<TValue> CreateList<TValue>(float width, float height) {
            return new ListBox<TValue>() { Width = width, Height = height };
        }

        private static Label CreateLabel(string text, float width) {
            return new Label() {
                Text = text,
                Format = Style.BodyText,
                AutoResize = false,
                Width = width,
                Height = LayoutMetrics.LABEL_HEIGHT,
            };
        }

        private static TextField CreateTextField(float width) {
            return new GameInputBlockingTextField() { Width = width, Height = LayoutMetrics.CONTROL_HEIGHT };
        }

        private static BorderedButton CreateButton(string text, float width = 0f) {
            var button = new BorderedButton() { Text = text, Padding = Vector2.Zero, Height = BUTTON_HEIGHT };

            if (width > 0f) {
                button.Width = width;
            }

            return button;
        }

        /// <summary>
        /// A row of buttons, each taking an equal share of the width. Bordered buttons carry a wide fixed
        /// default size, so a row that does not size its members spills out of its pane.
        /// </summary>
        private static HudChain CreateButtonRow(float width, params BorderedButton[] buttons) {
            var row = new HudChain(false) {
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = BUTTON_HEIGHT,
            };

            foreach (var button in buttons) {
                row.Add(button, 1f);
            }

            return row;
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

        // ---- jobs ----

        private void RefreshJobs(PaintJob jobToSelect = null) {
            var jobs = Service != null ? Service.GetJobs() : null;
            var selection = jobToSelect ?? _loadedJob;

            _suppressWrites = true;
            _jobList.ClearEntries();

            if (jobs != null) {
                foreach (var job in jobs.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)) {
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

            // The workbench selection is what the hotkeys act on, so choosing here also aims them.
            Service?.SetActiveJob(_loadedJob);

            _suppressWrites = true;

            if (_loadedJob != null) {
                _loadedJob.EnsureRules();
                _jobNameField.Text = _loadedJob.Name;

                var options = _loadedJob.Options ?? (_loadedJob.Options = new PaintJobOptions());
                _includeSubgridsCheckbox.Value = options.IncludeSubgrids;
                _includeProjectedCheckbox.Value = options.IncludeProjectedGrids;
                _includePreviewCheckbox.Value = options.IncludePreviewGrids;
                _respectOwnershipCheckbox.Value = options.RespectOwnership;
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

            RefreshJobs(job);
            Mod.Static?.RefreshPaintJobs(job);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnCopyJob(object sender, EventArgs e) {
            if (_loadedJob == null || Service == null) {
                return;
            }

            var copy = _loadedJob.Clone();
            copy.Id = Guid.NewGuid();
            copy.Name = UniqueJobName(_loadedJob.Name);

            Service.SaveJob(copy);
            RefreshJobs(copy);
            Mod.Static?.RefreshPaintJobs(copy);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private string UniqueJobName(string name) {
            var jobs = Service.GetJobs();
            var candidate = ModText.BC_UI_CopyOfName.GetString(name);
            var index = 2;

            while (jobs.Any(job => string.Equals(job.Name, candidate, StringComparison.InvariantCultureIgnoreCase))) {
                candidate = string.Format("{0} {1}", ModText.BC_UI_CopyOfName.GetString(name), index);
                index++;
            }

            return candidate;
        }

        private void OnRemoveJob(object sender, EventArgs e) {
            if (_loadedJob == null || Service == null) {
                return;
            }

            Service.RemoveJob(_loadedJob);
            _loadedJob = null;

            RefreshJobs();
            Mod.Static?.RefreshPaintJobs();
            HudSoundUtils.PlaySound("HudLockingLost");
        }

        // ---- rules ----

        private void RefreshRules(PaintRule ruleToSelect = null) {
            var selection = ruleToSelect ?? _loadedRule;

            _suppressWrites = true;
            _ruleList.ClearEntries();

            if (_loadedJob != null) {
                _rulesLabel.Text = ModText.BC_UI_RulesOf.GetString(_loadedJob.Name);

                for (var i = 0; i < _loadedJob.Rules.Count; i++) {
                    _ruleList.Add(string.Format("{0}. {1}", i + 1, _loadedJob.Rules[i].Name), _loadedJob.Rules[i]);
                }
            } else {
                _rulesLabel.Text = string.Empty;
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

            // A job with no rules cannot match anything, so the last one stays.
            if (_loadedJob.Rules.Count <= 1) {
                _statusLabel.Text = ModText.BC_UI_Status_NeedsRule.GetString();
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

        // ---- inspector ----

        private void ShowTab(bool conditions) {
            _conditionsView.Visible = conditions;
            _paintView.Visible = !conditions;

            // The tab that is showing is the one you cannot press.
            _conditionsTabButton.InputEnabled = !conditions;
            _paintTabButton.InputEnabled = conditions;
        }

        private void LoadRule() {
            _suppressWrites = true;

            if (_loadedRule == null) {
                _inspectorLabel.Text = string.Empty;
                _ruleNameField.Text = string.Empty;
                _conditionTree.ClearEntries();
                _sourceSummaryLabel.Text = string.Empty;
                _suppressWrites = false;

                return;
            }

            _inspectorLabel.Text = ModText.BC_UI_RuleDetailsOf.GetString(_loadedRule.Name);
            _ruleNameField.Text = _loadedRule.Name;

            if (_loadedRule.Action == null) {
                _loadedRule.Action = new PaintRuleAction();
            }

            _applyColorCheckbox.Value = _loadedRule.Action.ApplyColor;
            _applySkinCheckbox.Value = _loadedRule.Action.ApplySkin;

            var color = _loadedRule.Action.TargetColor;
            _colorPicker.Value = new VRageMath.Color(color.R, color.G, color.B);

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

            // A single color and a source that works one out per block answer the same question, so only the
            // one in use is on screen.
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

        private void OnPaletteColorPicked(VRageMath.Color color) {
            _colorPicker.Value = color;
            WriteRule();
        }

        // ---- writing back ----

        /// <summary>
        /// Copies the editor onto the selected rule. Everything here edits in place, so this runs on every
        /// change rather than on a save button.
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

            var color = _colorPicker.Value;
            action.TargetColor = new ColorModel(color.R, color.G, color.B);

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
            options.RespectOwnership = _respectOwnershipCheckbox.Value;

            Service?.Save();
        }

        /// <summary>
        /// Names are committed as the field loses interest rather than per keystroke, which keeps the lists
        /// from being rebuilt under the cursor while a name is being typed.
        /// </summary>
        private void CommitNames() {
            if (_loadedJob != null) {
                var jobName = _jobNameField.Text.ToString().Trim();

                if (!string.IsNullOrEmpty(jobName) && jobName != _loadedJob.Name) {
                    _loadedJob.Name = jobName;
                    Service?.Save();
                    RefreshJobs(_loadedJob);
                    Mod.Static?.RefreshPaintJobs(_loadedJob);
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

        // ---- actions ----

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
            if (_loadedRule == null || _activeDialog != null) {
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
            if (_loadedRule == null || _activeDialog != null) {
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

        private void OpenDialog(DialogBase dialog) {
            _activeDialog = dialog;
            dialog.Closed += OnDialogClosed;

            RequestDialog(dialog);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnDialogClosed(object sender, EventArgs e) {
            var dialog = sender as DialogBase;
            if (dialog != null) {
                dialog.Closed -= OnDialogClosed;
            }

            _activeDialog = null;
            RefreshConditionTree();
            UpdateSourceViews();
        }

        private void UpdateButtonState() {
            var hasJob = _loadedJob != null;
            var hasRule = _loadedRule != null;

            _copyJobButton.InputEnabled = hasJob;
            _removeJobButton.InputEnabled = hasJob;
            _applyButton.InputEnabled = hasJob;
            _addRuleButton.InputEnabled = hasJob;

            _removeRuleButton.InputEnabled = hasRule && _loadedJob != null && _loadedJob.Rules.Count > 1;
            _moveRuleUpButton.InputEnabled = hasRule;
            _moveRuleDownButton.InputEnabled = hasRule;
            _editConditionsButton.InputEnabled = hasRule;
            _editSourceButton.InputEnabled = hasRule;

            _undoButton.InputEnabled = (Service?.UndoCount ?? 0) > 0;

            _statusLabel.Text = hasJob
                ? ModText.BC_UI_WorkbenchStatus.GetString(_loadedJob.Rules.Count)
                : ModText.BC_NoPaintJobsAvailable.GetString();
        }

        protected override void HandleInput(Vector2 cursorPos) {
            base.HandleInput(cursorPos);

            if (_activeDialog != null) {
                return;
            }

            // Names are typed into fields that keep their own focus, so the commit happens once the field
            // has been left rather than on every letter.
            if (SharedBinds.Enter.IsNewPressed || SharedBinds.LeftButton.IsNewPressed) {
                CommitNames();
            }
        }
    }
}
