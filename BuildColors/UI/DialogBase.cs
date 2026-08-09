using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Base class for the mod's windows. Every one of them is dragged by its title bar, closed from the
    /// button in its corner, and sized and placed so that it keeps clear of the game's own HUD.
    /// </summary>
    public abstract class DialogBase : WindowBase {
        protected const float HEADER_HEIGHT = 63f;

        /// <summary>
        /// Space kept clear at the bottom of the screen. The game's toolbar, status readouts and crosshair
        /// live there, and a window over them hides the very build being painted.
        /// </summary>
        protected const float BOTTOM_RESERVE = 190f;

        /// <summary>
        /// Space kept clear at the top of the screen.
        /// </summary>
        protected const float TOP_MARGIN = 50f;

        private const float CLOSE_BUTTON_HEIGHT = 36f;
        private const float CLOSE_BUTTON_INSET = 12f;
        private const float CLOSE_BUTTON_WIDTH = 48f;

        /// <summary>
        /// Darkness of the backdrop each dialog draws behind itself. Nested dialogs stack their backdrops,
        /// so the deeper the stack, the darker everything below it becomes.
        /// </summary>
        private static readonly Color BackdropColor = new Color(0, 0, 0, 130);

        private readonly TexturedBox _backdrop;

        private bool _placed;

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

            // Resizing would have every dialog recompute a layout it worked out once from its own size, so
            // the size is fixed and only the position is the user's.
            AllowResizing = false;
            CanDrag = true;

            Padding = new Vector2(14f, 14f);

            var closeButton = new BorderedButton() {
                Text = ModText.BC_UI_CloseSymbol.GetString(),
                Padding = Vector2.Zero,
                Width = CLOSE_BUTTON_WIDTH,
                Height = CLOSE_BUTTON_HEIGHT,
                ParentAlignment = ParentAlignments.Right | ParentAlignments.Inner,
                Offset = new Vector2(-CLOSE_BUTTON_INSET, 0f),
            };

            closeButton.Register(header);
            closeButton.MouseInput.LeftClicked += (sender, args) => Close();
        }

        public event RichHudFramework.EventHandler Closed;

        public event RichHudFramework.EventHandler DialogRequested;

        /// <summary>
        /// The tallest a window may be without covering the game's HUD, given what it asked for. A window
        /// that cannot have both gives up the reserve rather than its own contents: contents that do not fit
        /// are drawn on top of each other, which is worse than sitting over the toolbar.
        /// </summary>
        protected static float GetSafeHeight(float preferred, float minimum) {
            var screen = DialogSafeArea.ScreenSize;
            var available = screen.Y - TOP_MARGIN - BOTTOM_RESERVE;
            var height = MathHelper.Clamp(preferred, Math.Min(minimum, available), Math.Max(available, minimum));

            return Math.Min(height, screen.Y - TOP_MARGIN * 2f);
        }

        /// <summary>
        /// Asks the host to display a nested dialog on top of this one.
        /// </summary>
        protected void RequestDialog(DialogBase dialog) {
            if (dialog == null) {
                return;
            }

            DialogRequested?.Invoke(this, new DialogRequestedEventArgs { Dialog = dialog });
        }

        /// <summary>
        /// Where the window sits when it first appears: horizontally in whatever room the color picker
        /// leaves, vertically in the band between the two margins. After this it stays where it is put,
        /// because from the first frame on the position belongs to whoever drags it.
        /// </summary>
        protected virtual Vector2 GetPlacement() {
            var screen = DialogSafeArea.ScreenSize;
            var centered = DialogSafeArea.GetCenterOffset(Size);

            // Centre of the band, unless the window is too tall for it, in which case its top edge is
            // pinned to the upper margin and the reserve at the bottom is what gives way.
            var banded = (BOTTOM_RESERVE - TOP_MARGIN) * .5f;
            var topAligned = screen.Y * .5f - TOP_MARGIN - Size.Y * .5f;

            return new Vector2(centered.X, Math.Min(banded, topAligned));
        }

        protected override void Layout() {
            base.Layout();

            // Placed on the first frame that has a size to place, and left alone from then on.
            if (!_placed && Size.X > 0f && Size.Y > 0f) {
                Offset = GetPlacement();
                _placed = true;
            }

            // The backdrop is positioned relative to the dialog, so cancel the dialog's own offset to keep
            // it centred on the screen regardless of where the dialog has been dragged.
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
