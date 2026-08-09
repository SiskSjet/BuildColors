using RichHudFramework.UI;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Text field that suppresses Space Engineers' own binds while it holds input focus.
    /// </summary>
    public class GameInputBlockingTextField : TextField {
        private string _lastText = string.Empty;
        private bool _wasOpen;

        public GameInputBlockingTextField(HudParentBase parent = null) : base(parent) {
            BindInput.InputFilter = SeBlacklistModes.AllKeys;
        }

        /// <summary>
        /// Raised on every keystroke, rather than only once focus is lost.
        /// </summary>
        public event Action<string> TextChanged;

        protected override void HandleInput(Vector2 cursorPos) {
            base.HandleInput(cursorPos);

            if (TextChanged == null || (!InputOpen && !_wasOpen)) {
                _wasOpen = InputOpen;
                return;
            }

            _wasOpen = InputOpen;

            var text = Text.ToString();

            if (text == _lastText) {
                return;
            }

            _lastText = text;
            TextChanged(text);
        }
    }
}
