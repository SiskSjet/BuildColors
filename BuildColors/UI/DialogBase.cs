using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Base class for modal dialogs. Dialogs are placed inside the free screen region and styled consistently.
    /// </summary>
    public abstract class DialogBase : WindowBase {
        protected const float HEADER_HEIGHT = 63f;

        /// <summary>
        /// Darkness of the backdrop each dialog draws behind itself. Nested dialogs stack their backdrops,
        /// so the deeper the stack, the darker everything below it becomes.
        /// </summary>
        private static readonly Color BackdropColor = new Color(0, 0, 0, 130);

        private readonly TexturedBox _backdrop;

        public DialogBase(HudParentBase parent = null) : base(parent) {
            // Dims whatever is behind this dialog, including a parent dialog when this one is nested.
            // It is a child of the dialog so it shares its draw layer, and it ignores masking so it can
            // extend past the window bounds to cover the screen.
            _backdrop = new TexturedBox(this) {
                Color = BackdropColor,
                ZOffset = -3,
                CanIgnoreMasking = true,
            };

            // Dialogs stay fully opaque. The game's UI background opacity is deliberately not applied here:
            // a translucent dialog over the color picker is hard to read.
            BodyColor = Style.BodyBackgroundColor.SetAlphaPct(1f);
            BorderColor = Style.BorderColor.SetAlphaPct(1f);

            header.Background.Color = Style.BodyBackgroundColor.SetAlphaPct(1f);
            header.textElement.Offset = new Vector2(0, -10);
            header.Format = Style.HeaderText;
            header.Height = HEADER_HEIGHT;
            
            AllowResizing = false;
            CanDrag = false;
            
            // Dialogs don't need drag or resize
            Padding = new Vector2(14f, 14f);
        }

        public event RichHudFramework.EventHandler Closed;

        public event RichHudFramework.EventHandler DialogRequested;

        /// <summary>
        /// Asks the host to display a nested dialog on top of this one.
        /// </summary>
        protected void RequestDialog(DialogBase dialog) {
            if (dialog == null) {
                return;
            }

            DialogRequested?.Invoke(this, new DialogRequestedEventArgs { Dialog = dialog });
        }

        protected override void Layout() {
            base.Layout();
            Offset = DialogSafeArea.GetCenterOffset(Size);

            // The backdrop is positioned relative to the dialog, so cancel the dialog's own offset to keep
            // it centred on the screen regardless of where the dialog sits.
            _backdrop.Size = DialogSafeArea.ScreenSize;
            _backdrop.Offset = -Offset;
        }

        /// <summary>
        /// Call this when the dialog should close
        /// </summary>
        protected void Close() {
            Closed?.Invoke(this, System.EventArgs.Empty);
            Unregister();
        }
    }
}
