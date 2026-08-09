using RichHudFramework.UI;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// One page of the panel.
    /// </summary>
    internal abstract class PanelView : HudElementBase {
        protected PanelView(float width, float height, HudParentBase parent = null) : base(parent) {
            Size = new Vector2(width, height);
        }

        /// <summary>
        /// Asks the host to put a dialog on screen over the panel.
        /// </summary>
        public event RichHudFramework.EventHandler DialogRequested;

        /// <summary>
        /// The dialog this view put on screen, or null while it has none.
        /// </summary>
        protected DialogBase ActiveDialog { get; private set; }

        /// <summary>
        /// Rebuilds the view from the current data.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Commits anything half typed before the panel leaves the screen.
        /// </summary>
        public virtual void Commit() { }

        /// <summary>
        /// Puts a dialog on screen and holds it until it closes, so a second one cannot open over it.
        /// </summary>
        protected void OpenDialog(DialogBase dialog) {
            if (dialog == null) {
                return;
            }

            ActiveDialog = dialog;
            dialog.Closed += OnDialogClosed;

            RequestDialog(dialog);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        /// <summary>
        /// Called once the dialog this view opened has gone.
        /// </summary>
        protected virtual void DialogClosed() { }

        private void OnDialogClosed(object sender, EventArgs args) {
            var dialog = sender as DialogBase;

            if (dialog != null) {
                dialog.Closed -= OnDialogClosed;
            }

            ActiveDialog = null;
            DialogClosed();
        }

        private void RequestDialog(DialogBase dialog) {
            DialogRequested?.Invoke(this, new DialogRequestedEventArgs { Dialog = dialog });
        }
    }
}
