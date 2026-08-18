using RichHudFramework.UI;
using RichHudFramework.UI.Client;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// What the user is asking a reorder session to do this frame.
    /// </summary>
    internal enum ReorderIntent {
        None,
        Previous,
        Next,
        Shallower,
        Deeper,
        Drop,
        Cancel
    }

    /// <summary>
    /// Input for picking a list item up, moving it and putting it down.
    /// </summary>
    internal static class ReorderInput {
        /// <summary>
        /// Renamed away from "BuildColors" so the saved bind group that still carries the old gamepad
        /// aliases is left behind rather than restored over these defaults.
        /// </summary>
        private const string GROUP_NAME = "BuildColorsReorder";

        private static IBindGroup _binds;
        private static BindDefinition[] _defaultBinds;
        private static IBind _grab;
        private static IBind _cancel;
        private static IBind _previous;
        private static IBind _next;
        private static IBind _shallower;
        private static IBind _deeper;

        /// <summary>
        /// Drops the cached handles so the binds are rebuilt after the framework resets.
        /// </summary>
        public static void Reset() {
            _binds = null;
            _defaultBinds = null;
            _grab = null;
            _cancel = null;
            _previous = null;
            _next = null;
            _shallower = null;
            _deeper = null;
        }

        /// <summary>
        /// The bind group, so it can be listed on a rebind page.
        /// </summary>
        public static IBindGroup Binds {
            get {
                EnsureBinds();
                return _binds;
            }
        }

        /// <summary>
        /// The bindings as first registered, which the rebind page offers as the reset target.
        /// </summary>
        public static BindDefinition[] DefaultBinds {
            get {
                EnsureBinds();
                return _defaultBinds;
            }
        }

        /// <summary>
        /// True on the frame the user asks to pick up or put down the selected item.
        /// </summary>
        public static bool GrabPressed {
            get {
                EnsureBinds();
                return _grab != null && _grab.IsNewPressed;
            }
        }

        /// <summary>
        /// Polls the movement binds.
        /// </summary>
        public static ReorderIntent Poll() {
            EnsureBinds();

            if (_binds == null) {
                return ReorderIntent.None;
            }

            if (_cancel.IsNewPressed) {
                return ReorderIntent.Cancel;
            }

            if (_grab.IsNewPressed) {
                return ReorderIntent.Drop;
            }

            if (IsTriggered(_previous)) {
                return ReorderIntent.Previous;
            }

            if (IsTriggered(_next)) {
                return ReorderIntent.Next;
            }

            if (IsTriggered(_shallower)) {
                return ReorderIntent.Shallower;
            }

            if (IsTriggered(_deeper)) {
                return ReorderIntent.Deeper;
            }

            return ReorderIntent.None;
        }

        private static bool IsTriggered(IBind bind) {
            return bind.IsNewPressed || bind.IsPressedAndHeld;
        }

        private static void EnsureBinds() {
            if (_binds != null) {
                return;
            }

            _binds = BindManager.GetOrCreateGroup(GROUP_NAME);
            if (_binds == null) {
                return;
            }

            _binds.RegisterBinds(new BindGroupInitializer {
                { "reorderGrab", RichHudControls.Space },
                { "reorderCancel", RichHudControls.Escape },
                { "reorderPrevious", RichHudControls.Up },
                { "reorderNext", RichHudControls.Down },
                { "reorderShallower", RichHudControls.Left },
                { "reorderDeeper", RichHudControls.Right },
            });

            _defaultBinds = _binds.GetBindDefinitions();

            _grab = _binds["reorderGrab"];
            _cancel = _binds["reorderCancel"];
            _previous = _binds["reorderPrevious"];
            _next = _binds["reorderNext"];
            _shallower = _binds["reorderShallower"];
            _deeper = _binds["reorderDeeper"];
        }
    }
}
