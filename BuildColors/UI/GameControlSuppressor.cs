using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Input;
using VRage.ModAPI;
using VRage.Utils;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Turns off the few game controls a mod bind would otherwise trigger next to its own action.
    /// <para>
    /// Replaces a blanket input blacklist, which also swallows unrelated shortcuts such as Alt + F10.
    /// </para>
    /// </summary>
    internal static class GameControlSuppressor {

        /// <summary>
        /// Highest control id Rich HUD still maps onto a keyboard key.
        /// </summary>
        private const int MAX_KEY_ID = 254;

        /// <summary>
        /// Every control the game binds a key to, as MyControlsSpace spells them.
        /// <para>
        /// The mod API can only look a control up by name or hand out the first control on a key, so
        /// finding every collision on a key means walking the whole list.
        /// </para>
        /// <para>
        /// Gamepad only controls are left out on purpose. IsEnabled turns a control off for every device
        /// bound to it, so nothing here may name one a keyboard key can never reach.
        /// </para>
        /// <para>
        /// The held controls are left out as well, see <see cref="HELD_CONTROLS" />.
        /// </para>
        /// </summary>
        private static readonly string[] GAME_CONTROLS = {
            "MAIN_MENU", "SWITCH_WALK", "USE", "PICK_UP",
            "PRIMARY_TOOL_ACTION", "SECONDARY_TOOL_ACTION",
            "TERMINAL", "REMOTE_ACCESS_MENU", "HELP_SCREEN", "CONTROL_MENU", "FACTIONS_MENU", "SYSTEM_RADIAL_MENU",
            "TOOLBAR_RADIAL_MENU", "CAMERA_ZOOM_IN", "CAMERA_ZOOM_OUT", "ACTIVE_CONTRACT_SCREEN",
            "HEADLIGHTS", "SCREENSHOT", "SIGNALS_FULLY_VISIBLE", "TOGGLE_SIGNALS", "SWITCH_LEFT",
            "SWITCH_RIGHT", "CUBE_COLOR_CHANGE", "TOGGLE_REACTORS", "TOGGLE_REACTORS_ALL", "THRUSTS",
            "BUILD_PLANNER", "CONSUME_HEALTH", "CONSUME_ENERGY", "BUILD_PLANNER_DEPOSIT_ORE",
            "BUILD_PLANNER_ADD_COMPONNETS", "BUILD_PLANNER_WITHDRAW_COMPONENTS", "BUILD_SCREEN",
            "CUBE_ROTATE_VERTICAL_POSITIVE", "CUBE_ROTATE_VERTICAL_NEGATIVE", "CUBE_ROTATE_HORISONTAL_POSITIVE",
            "CUBE_ROTATE_HORISONTAL_NEGATIVE", "CUBE_ROTATE_ROLL_POSITIVE", "CUBE_ROTATE_ROLL_NEGATIVE",
            "SYMMETRY_SWITCH", "SYMMETRY_SWITCH_ALTERNATIVE", "SYMMETRY_SETUP_CANCEL", "SYMMETRY_SETUP_ADD",
            "SYMMETRY_SETUP_REMOVE", "USE_SYMMETRY", "SWITCH_COMPOUND", "SWITCH_BUILDING_MODE",
            "VOXEL_HAND_SETTINGS", "CUBE_BUILDER_CUBESIZE_MODE", "CUBE_DEFAULT_MOUNTPOINT", "SYMMETRY_SWITCH_MODE",
            "SLOT1", "SLOT2", "SLOT3", "SLOT4", "SLOT5", "SLOT6", "SLOT7", "SLOT8", "SLOT9", "SLOT0", "PAGE1",
            "PAGE2", "PAGE3", "PAGE4", "PAGE5", "PAGE6", "PAGE7", "PAGE8", "PAGE9", "PAGE0", "TOOLBAR_UP",
            "TOOLBAR_DOWN", "TOOLBAR_NEXT_ITEM", "TOOLBAR_PREV_ITEM", "TOGGLE_HUD", "DAMPING", "DAMPING_RELATIVE",
            "CAMERA_MODE", "BROADCASTING", "HELMET", "CHAT_SCREEN", "CONSOLE", "SUICIDE", "LANDING_GEAR",
            "INVENTORY", "PAUSE_GAME", "SPECTATOR_NONE", "SPECTATOR_DELTA", "SPECTATOR_FREE", "SPECTATOR_STATIC",
            "VOICE_CHAT", "SPECTATOR_LOCK", "SPECTATOR_SWITCHMODE", "SPECTATOR_NEXTPLAYER",
            "SPECTATOR_PREVPLAYER", "RELOAD", "BUILD_MODE", "NEXT_BLOCK_STAGE", "PREV_BLOCK_STAGE", "MOVE_CLOSER",
            "MOVE_FURTHER", "COPY_PASTE_ACTION", "COPY_PASTE_CANCEL", "CHANGE_ROTATION_AXIS", "ROTATE_AXIS_LEFT",
            "ROTATE_AXIS_RIGHT", "CREATE_BLUEPRINT", "CREATE_BLUEPRINT_DETACHED", "CREATE_BLUEPRINT_MAGNETIC_LOCKS",
            "COPY_OBJECT", "COPY_OBJECT_DETACHED", "COPY_OBJECT_MAGNETIC_LOCKS", "PASTE_OBJECT", "CUT_OBJECT",
            "CUT_OBJECT_DETACHED", "CUT_OBJECT_MAGNETIC_LOCKS", "DELETE_OBJECT", "DELETE_OBJECT_DETACHED",
            "DELETE_OBJECT_MAGNETIC_LOCKS", "ADMIN_MENU", "BLUEPRINTS_MENU", "SPAWN_MENU", "PROGRESSION_MENU",
            "PLAYERS_SCREEN", "VOXEL_PAINT", "VOXEL_REVERT", "VOXEL_FURTHER", "VOXEL_CLOSER", "VOXEL_SELECT",
            "VOXEL_MATERIAL_SELECT", "VOXEL_SCALE_UP", "VOXEL_SCALE_DOWN", "VOXEL_SELECT_SPHERE", "CUTSCENE_SKIPPER",
            "TOOL_UP", "TOOL_DOWN", "TOOL_LEFT", "TOOL_RIGHT", "ACTION_UP", "ACTION_DOWN", "ACTION_LEFT",
            "ACTION_RIGHT", "EMOTE_SWITCHER", "EMOTE_SWITCHER_LEFT", "EMOTE_SWITCHER_RIGHT", "EMOTE_SELECT_1",
            "EMOTE_SELECT_2", "EMOTE_SELECT_3", "EMOTE_SELECT_4", "TOOLBAR_PREVIOUS", "TOOLBAR_NEXT",
            "CYCLE_COLOR_LEFT", "CYCLE_COLOR_RIGHT", "CYCLE_SKIN_LEFT", "CYCLE_SKIN_RIGHT", "COPY_COLOR",
            "WARNING_SCREEN", "RECOLOR", "MEDIUM_COLOR_BRUSH", "LARGE_COLOR_BRUSH", "RECOLOR_WHOLE_GRID",
            "COLOR_PICKER", "QUICK_PICK_COLOR", "SPECTATOR_FOCUS_PLAYER", "SPECTATOR_PLAYER_CONTROL",
            "SPECTATOR_LOCK_TO_GRID", "SPECTATOR_TELEPORT", "SPECTATOR_CHANGE_SPEED_UP",
            "SPECTATOR_CHANGE_SPEED_DOWN", "SPECTATOR_CHANGE_ROTATION_SPEED_UP",
            "SPECTATOR_CHANGE_ROTATION_SPEED_DOWN", "EXPORT_MODEL", "QUICK_LOAD_RECONNECT", "QUICK_SAVE",
            "BUFFS_SHOW_ALL",
        };

        /// <summary>
        /// Controls held rather than tapped. Never suppressed, and no default bind sits on their keys.
        /// </summary>
        private static readonly string[] HELD_CONTROLS = {
            "FORWARD", "BACKWARD", "STRAFE_LEFT", "STRAFE_RIGHT", "ROLL_LEFT", "ROLL_RIGHT", "SPRINT", "JUMP",
            "CROUCH", "LOOKAROUND", "LOOK_UP", "LOOK_DOWN", "LOOK_LEFT", "LOOK_RIGHT", "ROTATION_LEFT",
            "ROTATION_RIGHT", "ROTATION_UP", "ROTATION_DOWN", "FREE_ROTATION",
        };

        private static readonly List<IMyControl> _requested = new List<IMyControl>();
        private static readonly List<IMyControl> _suppressed = new List<IMyControl>();

        private static MyStringId[] _controlNames;

        /// <summary>
        /// Marks the given controls as unwanted for this frame.
        /// </summary>
        public static void Request(List<IMyControl> controls) {
            for (var i = 0; i < controls.Count; i++) {
                if (!_requested.Contains(controls[i])) {
                    _requested.Add(controls[i]);
                }
            }
        }

        /// <summary>
        /// Disables everything requested this frame and hands back whatever is no longer wanted.
        /// </summary>
        public static void Apply() {
            for (var i = _suppressed.Count - 1; i >= 0; i--) {
                var control = _suppressed[i];

                if (!_requested.Contains(control)) {
                    control.IsEnabled = true;
                    _suppressed.RemoveAt(i);
                }
            }

            for (var i = 0; i < _requested.Count; i++) {
                var control = _requested[i];

                if (!_suppressed.Contains(control)) {
                    _suppressed.Add(control);
                }

                control.IsEnabled = false;
            }

            _requested.Clear();
        }

        /// <summary>
        /// Hands every control back to the game.
        /// </summary>
        public static void RestoreAll() {
            for (var i = 0; i < _suppressed.Count; i++) {
                _suppressed[i].IsEnabled = true;
            }

            _suppressed.Clear();
            _requested.Clear();
        }

        /// <summary>
        /// Collects the game controls that fire on the given key while the given modifiers are held.
        /// <para>
        /// The game matches a bound modifier set exactly, so only the unmodified control and the one
        /// carrying the same modifiers can go off next to a mod bind.
        /// </para>
        /// </summary>
        public static void CollectConflicts(int controlId, MyKeyboardModifiers modifiers, List<IMyControl> result) {
            if (controlId <= 0 || controlId > MAX_KEY_ID || MyAPIGateway.Input == null) {
                return;
            }

            var key = (MyKeys)controlId;
            var names = EnsureControlNames();

            for (var i = 0; i < names.Length; i++) {
                var control = MyAPIGateway.Input.GetGameControl(names[i]);

                if (control == null) {
                    continue;
                }

                var collides = Collides(control.GetKeyboardControl(), control.GetKeyboardModifier(), key, modifiers)
                    || Collides(control.GetSecondKeyboardControl(), control.GetSecondKeyboardModifier(), key, modifiers);

                if (collides) {
                    Add(control, result);
                }
            }
        }

        /// <summary>
        /// The key the game has a control on.
        /// </summary>
        public static MyKeys GetKeyboardKey(string gameControl) {
            var input = MyAPIGateway.Input;

            if (input == null) {
                return MyKeys.None;
            }

            var control = input.GetGameControl(MyStringId.GetOrCompute(gameControl));

            if (control == null) {
                return MyKeys.None;
            }

            var key = control.GetKeyboardControl();

            return key != MyKeys.None ? key : control.GetSecondKeyboardControl();
        }

        /// <summary>
        /// Whether the player walks, flies or looks around with the given key.
        /// </summary>
        public static bool IsHeldControlKey(MyKeys key) {
            var input = MyAPIGateway.Input;

            if (key == MyKeys.None || input == null) {
                return false;
            }

            for (var i = 0; i < HELD_CONTROLS.Length; i++) {
                var control = input.GetGameControl(MyStringId.GetOrCompute(HELD_CONTROLS[i]));

                if (control == null) {
                    continue;
                }

                if (control.GetKeyboardControl() == key || control.GetSecondKeyboardControl() == key) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The modifier flag a control stands for, or none if it is an ordinary key.
        /// </summary>
        public static MyKeyboardModifiers GetModifier(int controlId) {
            switch ((MyKeys)controlId) {
                case MyKeys.Control:
                case MyKeys.LeftControl:
                case MyKeys.RightControl:
                    return MyKeyboardModifiers.Control;

                case MyKeys.Shift:
                case MyKeys.LeftShift:
                case MyKeys.RightShift:
                    return MyKeyboardModifiers.Shift;

                case MyKeys.Alt:
                case MyKeys.LeftAlt:
                case MyKeys.RightAlt:
                    return MyKeyboardModifiers.Alt;

                default:
                    return MyKeyboardModifiers.None;
            }
        }

        private static void Add(IMyControl control, List<IMyControl> result) {
            if (control != null && !result.Contains(control)) {
                result.Add(control);
            }
        }

        /// <summary>
        /// A key set off a control when it carries no modifiers at all, or exactly the ones held.
        /// </summary>
        private static bool Collides(MyKeys boundKey, MyKeyboardModifiers boundModifiers, MyKeys key, MyKeyboardModifiers modifiers) {
            if (boundKey != key) {
                return false;
            }

            return boundModifiers == MyKeyboardModifiers.None || boundModifiers == modifiers;
        }

        private static MyStringId[] EnsureControlNames() {
            if (_controlNames == null) {
                _controlNames = new MyStringId[GAME_CONTROLS.Length];

                for (var i = 0; i < GAME_CONTROLS.Length; i++) {
                    _controlNames[i] = MyStringId.GetOrCompute(GAME_CONTROLS[i]);
                }
            }

            return _controlNames;
        }
    }
}
