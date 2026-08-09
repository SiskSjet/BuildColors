using RichHudFramework.UI;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Text field that suppresses Space Engineers' own binds while it holds input focus.
    /// <para>The vanilla ColorPick screen stays open behind our window and keeps polling game controls, so
    /// typing a letter would otherwise trigger the matching bind 'p' hits COLOR_PICKER and closes the
    /// screen. Blacklisting the controls only affects the game's control lookups, not the raw text input
    /// buffer, so the characters themselves still reach the field.</para>
    /// </summary>
    public class GameInputBlockingTextField : TextField {

        public GameInputBlockingTextField(HudParentBase parent = null) : base(parent) {
            BindInput.InputFilter = SeBlacklistModes.AllKeys;
        }
    }
}
