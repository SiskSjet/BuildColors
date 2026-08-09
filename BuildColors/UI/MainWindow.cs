using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using System.Collections.Generic;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Full-screen transparent container that holds all UI panels and dialogs.
    /// Ensures proper z-ordering and owns the modal dialog stack. Each dialog draws its own backdrop.
    /// </summary>
    public class MainWindow : HudElementBase {
        private const sbyte DIALOG_Z_OFFSET = 20;

        private readonly ColorManagementPanel _colorPanel;
        private readonly PaintJobPanel _paintJobPanel;
        private readonly List<DialogBase> _dialogStack = new List<DialogBase>();

        public MainWindow(HudParentBase parent = null) : base(parent) {
            // Full-screen transparent container
            DimAlignment = DimAlignments.Both;
            
            // Right panel - Color Management
            _colorPanel = new ColorManagementPanel() {
                ZOffset = 1,
            };
            _colorPanel.Register(this);

            // Left panel - Paint Jobs
            _paintJobPanel = new PaintJobPanel() {
                ZOffset = 1,
            };
            _paintJobPanel.Register(this);

            // Wire up dialog requests
            _colorPanel.DialogRequested += OnDialogRequested;
            _paintJobPanel.DialogRequested += OnDialogRequested;
        }

        public ColorManagementPanel ColorPanel => _colorPanel;
        public PaintJobPanel PaintJobPanel => _paintJobPanel;

        /// <summary>
        /// True while any dialog is open. The workbench stands on its own, so the container has to stay up
        /// for it even when the colour picker screen that normally hosts the panels is closed.
        /// </summary>
        public bool HasOpenDialogs => _dialogStack.Count > 0;

        /// <summary>
        /// The two side panels belong to the colour picker screen and are hidden when it is not up.
        /// </summary>
        public bool PanelsVisible {
            set {
                _colorPanel.Visible = value;
                _paintJobPanel.Visible = value;
            }
        }

        /// <summary>
        /// Shows a dialog as a modal overlay. Dialogs stack, so a dialog can open a nested dialog on top of itself.
        /// </summary>
        public void ShowDialog(DialogBase dialog) {
            if (dialog == null || _dialogStack.Contains(dialog)) {
                return;
            }

            // Only the topmost dialog accepts input. Without this a click on a dialog underneath would
            // pull window focus and bury the dialog the user is actually working in.
            if (_dialogStack.Count > 0) {
                _dialogStack[_dialogStack.Count - 1].InputEnabled = false;
            }

            dialog.ZOffset = DIALOG_Z_OFFSET;
            dialog.Closed += OnDialogClosed;
            dialog.DialogRequested += OnDialogRequested;
            dialog.Register(this);
            dialog.InputEnabled = true;

            // WindowBase tracks its own draw layer, so the newest dialog has to claim focus explicitly.
            dialog.GetWindowFocus();

            _dialogStack.Add(dialog);
        }

        protected override void Layout() {
            base.Layout();

            // This element lives under the high DPI root, so it has to match that coordinate space rather
            // than the raw viewport, otherwise dialog backdrops are oversized on high DPI displays.
            Size = DialogSafeArea.ScreenSize;
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

            // A dialog closing also discards anything stacked on top of it.
            for (var i = _dialogStack.Count - 1; i >= index; i--) {
                var dialog = _dialogStack[i];
                dialog.Closed -= OnDialogClosed;
                dialog.DialogRequested -= OnDialogRequested;

                if (dialog != closed) {
                    dialog.Unregister();
                }

                _dialogStack.RemoveAt(i);
            }

            // Hand input and focus back to whatever is now on top.
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
