using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using System.Collections.Generic;
using Sisk.Utils.Localization.Extensions;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Full-screen transparent container that holds the panel and the modal dialog stack.
    /// </summary>
    public class MainWindow : HudElementBase {
        private const sbyte DIALOG_Z_OFFSET = 20;
        private const float CHIP_WIDTH = 368f;
        private const float CHIP_HEIGHT = 48f;
        private const float CHIP_INSET = 14f;
        private const float CHIP_LABEL_WIDTH = 128f;
        private const float RESTORE_BUTTON_WIDTH = 204f;

        private readonly List<DialogBase> _dialogStack = new List<DialogBase>();
        private TexturedBox _chip;
        private BuildColorsPanel _panel;

        public MainWindow(HudParentBase parent = null) : base(parent) {
            DimAlignment = DimAlignments.Both;

            BuildPanel();
            BuildChip();
        }

        /// <summary>
        /// True while any dialog is open.
        /// </summary>
        public bool HasOpenDialogs => _dialogStack.Count > 0;

        /// <summary>
        /// Whether the panel is tucked away behind the chip. It outlives Rich HUD resets, so the panel
        /// comes back the way the player left it.
        /// </summary>
        public static bool Minimized;

        /// <summary>
        /// The panel belongs to the color picker screen and is hidden whenever that screen is not up.
        /// </summary>
        public bool PanelVisible {
            get { return _panel != null && _panel.Visible; }
            set {
                if (_panel == null || _panel.Visible == value) {
                    return;
                }

                _panel.Visible = value;

                if (value) {
                    _panel.Reload();
                } else {
                    _panel.Commit();
                }
            }
        }

        public void RefreshPaintJobs(PaintJob jobToSelect = null) {
            _panel?.RefreshPaintJobs(jobToSelect);
        }

        public void RefreshShares() {
            _panel?.RefreshShares();
        }

        /// <summary>
        /// Rebuilds the panel for the current screen size.
        /// </summary>
        public void RebuildIfScreenChanged() {
            if (_panel == null || HasOpenDialogs) {
                return;
            }

            var size = PickerScreenLayout.PanelSize;
            var built = _panel.BuiltForSize;

            if (built.X <= 0f || built.Y <= 0f) {
                return;
            }

            if (Vector2.DistanceSquared(size, built) < 1f) {
                return;
            }

            var wasVisible = _panel.Visible;

            _panel.Commit();
            _panel.DialogRequested -= OnDialogRequested;
            _panel.MinimizeRequested -= OnMinimizeRequested;
            _panel.Unregister();

            BuildPanel();
            _panel.Visible = wasVisible;
        }

        /// <summary>
        /// Shows a dialog as a modal overlay.
        /// </summary>
        public void ShowDialog(DialogBase dialog) {
            if (dialog == null || _dialogStack.Contains(dialog)) {
                return;
            }

            if (_dialogStack.Count > 0) {
                _dialogStack[_dialogStack.Count - 1].InputEnabled = false;
            } else if (_panel != null) {
                _panel.InputEnabled = false;
            }

            dialog.ZOffset = DIALOG_Z_OFFSET;
            dialog.Closed += OnDialogClosed;
            dialog.DialogRequested += OnDialogRequested;
            dialog.Register(this);
            dialog.InputEnabled = true;

            dialog.GetWindowFocus();

            _dialogStack.Add(dialog);
        }

        protected override void Layout() {
            base.Layout();

            Size = DialogSafeArea.ScreenSize;

            _chip.Offset = new Vector2(
                -Size.X * .5f + LayoutMetrics.SCREEN_GAP + CHIP_WIDTH * .5f,
                -Size.Y * .5f + LayoutMetrics.SCREEN_GAP + CHIP_HEIGHT * .5f);
        }

        private void BuildPanel() {
            _panel = new BuildColorsPanel() { ZOffset = 1 };
            _panel.DialogRequested += OnDialogRequested;
            _panel.MinimizeRequested += OnMinimizeRequested;
            _panel.Register(this);
        }

        /// <summary>
        /// Tucks the panel away behind the chip.
        /// </summary>
        public void Minimize() {
            if (Minimized || _panel == null) {
                return;
            }

            Minimized = true;
            _chip.Visible = true;
            _chip.InputEnabled = true;
            PanelVisible = false;
        }

        /// <summary>
        /// Brings the panel back from the chip.
        /// </summary>
        public void Restore() {
            if (!Minimized || _panel == null) {
                return;
            }

            Minimized = false;
            _chip.Visible = false;
            _chip.InputEnabled = false;
            PanelVisible = true;
        }

        private void BuildChip() {
            _chip = new TexturedBox() {
                Size = new Vector2(CHIP_WIDTH, CHIP_HEIGHT),
                Color = Style.BodyBackgroundColor,
                ZOffset = 2,
                Visible = Minimized,
                InputEnabled = Minimized,
            };

            new BorderBox(_chip) {
                DimAlignment = DimAlignments.Both,
                Color = Style.BorderColor,
                Thickness = 1f,
            };

            var title = new Label() {
                Text = ModText.BC_UI_PanelTitle.GetString(),
                Format = Style.BodyText,
                AutoResize = false,
                Width = CHIP_LABEL_WIDTH,
                Height = CHIP_HEIGHT,
                ParentAlignment = ParentAlignments.Left | ParentAlignments.Inner,
                Offset = new Vector2(CHIP_INSET, 0f),
            };

            title.Register(_chip);

            var restoreButton = ControlFactory.CreateButton(ModText.BC_UI_Restore.GetString(), RESTORE_BUTTON_WIDTH);
            restoreButton.Height = LayoutMetrics.BUTTON_HEIGHT;
            restoreButton.ParentAlignment = ParentAlignments.Right | ParentAlignments.Inner;
            restoreButton.Offset = new Vector2(-CHIP_INSET, 0f);

            restoreButton.MouseInput.LeftClicked += (sender, args) => {
                HudSoundUtils.PlaySound("HudMouseClick");
                Restore();
            };

            restoreButton.Register(_chip);
            _chip.Register(this);
        }

        private void OnMinimizeRequested(object sender, System.EventArgs e) {
            Minimize();
        }

        protected override void Draw() {
            base.Draw();

            var opacity = MyAPIGateway.Session?.Config?.UIBkOpacity ?? 1f;
            _chip.Color = Style.BodyBackgroundColor.SetAlphaPct(opacity);
        }

        private void OnDialogRequested(object sender, System.EventArgs e) {
            var args = e as DialogRequestedEventArgs;
            if (args != null) {
                ShowDialog(args.Dialog);
            }
        }

        private void OnDialogClosed(object sender, System.EventArgs e) {
            var closed = sender as DialogBase;
            if (closed == null) {
                return;
            }

            var index = _dialogStack.IndexOf(closed);
            if (index < 0) {
                return;
            }

            for (var i = _dialogStack.Count - 1; i >= index; i--) {
                var dialog = _dialogStack[i];
                dialog.Closed -= OnDialogClosed;
                dialog.DialogRequested -= OnDialogRequested;

                if (dialog != closed) {
                    dialog.Unregister();
                }

                _dialogStack.RemoveAt(i);
            }

            if (_dialogStack.Count > 0) {
                var top = _dialogStack[_dialogStack.Count - 1];
                top.InputEnabled = true;
                top.GetWindowFocus();
            } else if (_panel != null && !Minimized) {
                _panel.InputEnabled = true;
            }
        }
    }

    /// <summary>
    /// Event args for dialog requests from panels
    /// </summary>
    public class DialogRequestedEventArgs : System.EventArgs {
        public DialogBase Dialog { get; set; }

        public DialogRequestedEventArgs() { }
    }
}
