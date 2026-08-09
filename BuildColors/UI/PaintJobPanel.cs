using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Linq;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Left-side panel for paint job management.
    /// Styled like a window but implemented as a panel component.
    /// </summary>
    public class PaintJobPanel : HudElementBase {
        private const float BUTTON_HEIGHT = 50f;
        private const float BUTTON_SPACING = 8f;
        private const float HEIGHT = 1080f;
        private const float WIDTH = 500f;
        private const float WINDOW_GAP = 30f;

        /// <summary>
        /// Width left for content once the horizontal padding of the main layout is taken off.
        /// </summary>
        private const float CONTENT_WIDTH = WIDTH - 2f * CONTENT_PADDING_X;

        private const float CONTENT_PADDING_X = 10f;

        private readonly BorderedButton _applyJobButton;
        private readonly TexturedBox _border;
        private readonly TexturedBox _body;
        private readonly BorderedButton _editJobButton;
        private readonly Label _header;
        private readonly BorderedButton _newJobButton;
        private readonly BorderedButton _refreshButton;
        private readonly BorderedButton _removeJobButton;
        private readonly BorderedButton _undoButton;
        private readonly ListBox<PaintJob> _jobList;

        public PaintJobPanel(HudParentBase parent = null) : base(parent) {
            Size = new Vector2(WIDTH, HEIGHT);

            // Window-style border
            _border = new TexturedBox() {
                DimAlignment = DimAlignments.Both,
                Color = Style.BorderColor,
            };
            _border.Register(this);

            // Window-style body background
            _body = new TexturedBox() {
                DimAlignment = DimAlignments.Both,
                Color = Style.BodyBackgroundColor,
                Padding = new Vector2(2f),
            };
            _body.Register(this);

            // Header
            var headerBackground = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = 20,
                Color = VRageMath.Color.Transparent,
            };

            _header = new Label() {
                Text = ModText.BC_UI_PaintJobs.GetString(),
                Format = Style.HeaderText,
                Padding = new Vector2(50f, 0f),
            };

            var headerSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            var label = new Label() {
                Text = ModText.BC_UI_PaintJobs.GetString(),
                ParentAlignment = ParentAlignments.Left
            };

            _jobList = new ListBox<PaintJob>() {
                DimAlignment = DimAlignments.Width,
                Height = 250f,
            };

            var jobListSeperator = new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Color = Style.SeparatorColor,
            };

            _newJobButton = new BorderedButton() {
                Text = ModText.BC_UI_New.GetString(),
                Padding = Vector2.Zero
            };

            _editJobButton = new BorderedButton() {
                Text = ModText.BC_UI_Edit.GetString(),
                Padding = Vector2.Zero,
                InputEnabled = false
            };

            _removeJobButton = new BorderedButton() {
                Text = ModText.BC_UI_Remove.GetString(),
                Padding = Vector2.Zero,
                InputEnabled = false
            };

            _applyJobButton = new BorderedButton() {
                Text = ModText.BC_UI_Apply.GetString(),
                Padding = Vector2.Zero,
                InputEnabled = false
            };

            _refreshButton = new BorderedButton() {
                Text = ModText.BC_UI_Refresh.GetString(),
                Padding = Vector2.Zero,
            };

            _undoButton = new BorderedButton() {
                Text = ModText.BC_UI_Undo.GetString(),
                Padding = Vector2.Zero,
                InputEnabled = false
            };

            // Two to a row, each taking half of it. A bordered button carries a fixed default width wider
            // than half this panel, so a row that does not size its members overflows the panel instead of
            // wrapping - which is what a third button in a row did.
            var jobListButtons = new HudChain(true) {
                CollectionContainer = {
                    CreateButtonRow(_newJobButton, _removeJobButton),
                    CreateButtonRow(_editJobButton, _refreshButton),
                    CreateButtonRow(_applyJobButton, _undoButton)
                },
                Spacing = 10f,
            };

            var headerLayout = new HudChain(true) {
                CollectionContainer = { headerBackground, _header, headerSeperator },
                Spacing = 10f,
            };

            var jobListLayout = new HudChain(true) {
                CollectionContainer = { label, _jobList, jobListSeperator, jobListButtons },
                Spacing = 10f,
            };

            var mainLayout = new HudChain(true, _body) {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV,
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
                CollectionContainer = { headerLayout, jobListLayout },
                Spacing = 10f,
                Padding = new Vector2(10f, 0f),
            };

            _jobList.ValueChanged += OnJobChanged;
            _jobList.MouseInput.CursorEntered += OnMouseOver;

            _newJobButton.MouseInput.LeftClicked += OnNewJobClicked;
            _newJobButton.MouseInput.CursorEntered += OnMouseOver;

            _editJobButton.MouseInput.LeftClicked += OnEditJobClicked;
            _editJobButton.MouseInput.CursorEntered += OnMouseOver;

            _removeJobButton.MouseInput.LeftClicked += OnRemoveJobClicked;
            _removeJobButton.MouseInput.CursorEntered += OnMouseOver;

            _applyJobButton.MouseInput.LeftClicked += OnApplyJobClicked;
            _applyJobButton.MouseInput.CursorEntered += OnMouseOver;

            _undoButton.MouseInput.LeftClicked += OnUndoClicked;
            _undoButton.MouseInput.CursorEntered += OnMouseOver;

            _refreshButton.MouseInput.LeftClicked += (s, e) => Refresh();
            _refreshButton.MouseInput.CursorEntered += OnMouseOver;

            Refresh();
        }

        /// <summary>
        /// Builds a row of two buttons, each given half the width. Weighted members rather than their own
        /// size, so the row stays inside the panel whatever the buttons default to.
        /// </summary>
        private static HudChain CreateButtonRow(BorderedButton left, BorderedButton right) {
            return new HudChain(false) {
                CollectionContainer = { { left, 1f }, { right, 1f } },
                Spacing = BUTTON_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = CONTENT_WIDTH,
                Height = BUTTON_HEIGHT,
            };
        }

        public void Refresh(PaintJob jobToSelect = null) {
            UpdateUndoState();

            var selection = jobToSelect ?? _jobList.Value?.AssocMember ?? Mod.Static?.PaintJobService?.ActiveJob;

            _jobList.ClearEntries();

            var jobs = Mod.Static?.PaintJobService?.GetJobs();
            if (jobs == null || jobs.Count == 0) {
                _applyJobButton.InputEnabled = false;
                _editJobButton.InputEnabled = false;
                _removeJobButton.InputEnabled = false;
                return;
            }

            var ordered = jobs.OrderBy(r => r.Name, StringComparer.InvariantCultureIgnoreCase).ToArray();
            foreach (var paintJob in ordered) {
                _jobList.Add(paintJob.Name, paintJob);
            }

            var index = selection != null ? Array.FindIndex(ordered, r => r.Id == selection.Id) : 0;
            _jobList.SetSelectionAt(index >= 0 ? index : 0);
        }

        private void OnApplyJobClicked(object sender, EventArgs e) {
            var job = _jobList.Value?.AssocMember;
            if (job == null) {
                return;
            }

            Mod.Static?.PaintJobService?.ApplyJobToSelection(job);
            UpdateUndoState();
            HudSoundUtils.PlaySound("HudBleep");
        }

        private void OnUndoClicked(object sender, EventArgs e) {
            Mod.Static?.PaintJobService?.UndoPaintJob();
            UpdateUndoState();
            HudSoundUtils.PlaySound("HudLockingLost");
        }

        /// <summary>
        /// The undo button follows the history rather than the selected job: what it puts back is the last
        /// thing painted, whichever job that was.
        /// </summary>
        private void UpdateUndoState() {
            _undoButton.InputEnabled = (Mod.Static?.PaintJobService?.UndoCount ?? 0) > 0;
        }

        private void OnEditJobClicked(object sender, EventArgs e) {
            var job = _jobList.Value?.AssocMember;
            if (job == null) {
                return;
            }

            // Editing happens in the workbench, which shows the job, its rules and the selected rule at
            // once instead of burying each behind the one before it.
            Mod.Static?.PaintJobService?.SetActiveJob(job);
            Mod.Static?.OpenWorkbench();
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnMouseOver(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudMouseOver");
        }

        private void OnNewJobClicked(object sender, EventArgs e) {
            var service = Mod.Static?.PaintJobService;
            if (service == null) {
                return;
            }

            var job = service.CreateJob();
            Refresh(job);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnRemoveJobClicked(object sender, EventArgs e) {
            var job = _jobList.Value?.AssocMember;
            if (job == null) {
                return;
            }

            var service = Mod.Static?.PaintJobService;
            if (service == null) {
                return;
            }

            var removed = service.RemoveJob(job);
            if (removed) {
                Refresh();
                HudSoundUtils.PlaySound("HudLockingLost");
            }
        }

        private void OnJobChanged(object sender, EventArgs e) {
            var job = _jobList.Value?.AssocMember;

            // The panel selection is what the hotkeys act on, so picking here is also how you choose the job
            // for a keypress made later with nothing on screen.
            Mod.Static?.PaintJobService?.SetActiveJob(job);

            if (job == null) {
                _applyJobButton.InputEnabled = false;
                _editJobButton.InputEnabled = false;
                _removeJobButton.InputEnabled = false;
                return;
            }

            _applyJobButton.InputEnabled = true;
            _editJobButton.InputEnabled = true;
            _removeJobButton.InputEnabled = true;
        }

        public event RichHudFramework.EventHandler DialogRequested;

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

            var anchorOffset = ColorManagementPanel.SLOPE * aspectRatio + ColorManagementPanel.Y_INTERCEPT;
            var leftOffset = anchorOffset - (ColorManagementPanel.WIDTH + WINDOW_GAP);
            Offset = new Vector2(leftOffset, 0f);
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