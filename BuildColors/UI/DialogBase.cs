using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Base class for the mod's windows.
    /// </summary>
    public abstract class DialogBase : WindowBase {
        protected const float HEADER_HEIGHT = 63f;

        /// <summary>
        /// Space kept clear at the bottom of the screen.
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
        /// Darkness of the backdrop each dialog draws behind itself.
        /// </summary>
        private static readonly Color BackdropColor = new Color(0, 0, 0, 130);

        private readonly TexturedBox _backdrop;

        private bool _placed;

        public DialogBase(HudParentBase parent = null) : base(parent) {
            _backdrop = new TexturedBox(this) {
                Color = BackdropColor,
                ZOffset = -3,
                CanIgnoreMasking = true,
            };

            BodyColor = Style.BodyBackgroundColor.SetAlphaPct(1f);
            BorderColor = Style.BorderColor.SetAlphaPct(1f);

            header.Background.Color = Style.ChromeBackgroundColor.SetAlphaPct(1f);
            header.textElement.Offset = new Vector2(0, -10);
            header.Format = Style.HeaderText;
            header.Height = HEADER_HEIGHT;

            AllowResizing = false;
            CanDrag = true;

            Padding = new Vector2(14f, 14f);

            var closeButton = ControlFactory.CreateButton(ModText.BC_UI_CloseSymbol.GetString(), CLOSE_BUTTON_WIDTH);
            closeButton.Height = CLOSE_BUTTON_HEIGHT;
            closeButton.ParentAlignment = ParentAlignments.Right | ParentAlignments.Inner;
            closeButton.Offset = new Vector2(-CLOSE_BUTTON_INSET, 0f);

            closeButton.Register(header);
            closeButton.MouseInput.LeftClicked += (sender, args) => Close();
        }

        public event RichHudFramework.EventHandler Closed;

        public event RichHudFramework.EventHandler DialogRequested;

        /// <summary>
        /// The tallest a window may be without covering the game's HUD, given what it asked for.
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
        /// Where the window sits when it first appears.
        /// </summary>
        protected virtual Vector2 GetPlacement() {
            var screen = DialogSafeArea.ScreenSize;
            var centered = DialogSafeArea.GetCenterOffset(Size);

            var banded = (BOTTOM_RESERVE - TOP_MARGIN) * .5f;
            var topAligned = screen.Y * .5f - TOP_MARGIN - Size.Y * .5f;

            return new Vector2(centered.X, Math.Min(banded, topAligned));
        }

        protected override void Layout() {
            base.Layout();

            if (!_placed && Size.X > 0f && Size.Y > 0f) {
                Offset = GetPlacement();
                _placed = true;
            }

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
