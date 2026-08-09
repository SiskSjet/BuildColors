using RichHudFramework.UI;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using System.Collections.Generic;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Full-screen transparent container that holds the panel and the modal dialog stack.
    /// </summary>
    public class MainWindow : HudElementBase {
        private const sbyte DIALOG_Z_OFFSET = 20;

        private readonly List<DialogBase> _dialogStack = new List<DialogBase>();

        private BuildColorsPanel _panel;

        public MainWindow(HudParentBase parent = null) : base(parent) {
            DimAlignment = DimAlignments.Both;

            BuildPanel();
        }

        /// <summary>
        /// True while any dialog is open.
        /// </summary>
        public bool HasOpenDialogs => _dialogStack.Count > 0;

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
        }

        private void BuildPanel() {
            _panel = new BuildColorsPanel() { ZOffset = 1 };
            _panel.DialogRequested += OnDialogRequested;
            _panel.Register(this);
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
