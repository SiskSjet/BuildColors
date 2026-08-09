using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Hotkeys that act on the world rather than on a dialog: apply the active paint job to whatever the
    /// player is looking at, put the last one back, and pick which job is active - all without the color
    /// picker being open.
    /// <para>
    /// These are driven by the bind's own events rather than polled from a dialog, because the whole point
    /// is that they work while no part of this mod's UI is on screen. Rich HUD raises them from its own
    /// update, so the mod does not need an update order of its own.
    /// </para>
    /// </summary>
    internal static class PaintJobInput {

        /// <summary>
        /// Kept apart from the editing binds so that the rebind page can list "what the keys do in the
        /// world" separately from "what the keys do in a list".
        /// </summary>
        private const string GROUP_NAME = "BuildColorsPaintJobs";

        private static IBindGroup _binds;
        private static BindDefinition[] _defaultBinds;
        private static IBind _apply;
        private static IBind _undo;
        private static IBind _nextJob;
        private static IBind _previousJob;
        private static IBind _openWorkbench;

        /// <summary>
        /// Registers the binds and starts listening. Called once Rich HUD is up, because until then there is
        /// no bind manager to register with.
        /// </summary>
        public static void Register() {
            EnsureBinds();
        }

        /// <summary>
        /// The bind group, so it can be listed on the rebind page.
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
        /// Rich HUD tears its API down on reset, so the handles and the subscriptions taken from it are
        /// dropped and rebuilt on the next init.
        /// </summary>
        public static void Reset() {
            Unsubscribe();

            _binds = null;
            _defaultBinds = null;
            _apply = null;
            _undo = null;
            _nextJob = null;
            _previousJob = null;
            _openWorkbench = null;
        }

        private static void EnsureBinds() {
            if (_binds != null) {
                return;
            }

            _binds = BindManager.GetOrCreateGroup(GROUP_NAME);
            if (_binds == null) {
                return;
            }

            // Two key combos throughout: a paint job repaints a whole grid, which is not something a single
            // stray keypress should be able to start.
            _binds.RegisterBinds(new BindGroupInitializer {
                { "paintApply", RichHudControls.Alt, RichHudControls.P },
                { "paintUndo", RichHudControls.Alt, RichHudControls.Z },
                { "paintNextJob", RichHudControls.Alt, RichHudControls.OemPeriod },
                { "paintPreviousJob", RichHudControls.Alt, RichHudControls.OemComma },
                { "paintWorkbench", RichHudControls.Alt, RichHudControls.B },
            });

            // Captured before any saved configuration can change them, so this really is the default set.
            _defaultBinds = _binds.GetBindDefinitions();

            _apply = _binds["paintApply"];
            _undo = _binds["paintUndo"];
            _nextJob = _binds["paintNextJob"];
            _previousJob = _binds["paintPreviousJob"];
            _openWorkbench = _binds["paintWorkbench"];

            Subscribe();
        }

        private static void Subscribe() {
            if (_apply == null) {
                return;
            }

            _apply.NewPressed += OnApply;
            _undo.NewPressed += OnUndo;
            _nextJob.NewPressed += OnNextJob;
            _previousJob.NewPressed += OnPreviousJob;
            _openWorkbench.NewPressed += OnOpenWorkbench;
        }

        private static void Unsubscribe() {
            if (_apply == null) {
                return;
            }

            _apply.NewPressed -= OnApply;
            _undo.NewPressed -= OnUndo;
            _nextJob.NewPressed -= OnNextJob;
            _previousJob.NewPressed -= OnPreviousJob;
            _openWorkbench.NewPressed -= OnOpenWorkbench;
        }

        private static void OnApply(object sender, System.EventArgs args) {
            if (!CanAct()) {
                return;
            }

            Mod.Static?.PaintJobService?.ApplyActiveJob();
            Mod.Static?.RefreshPaintJobs();
        }

        private static void OnUndo(object sender, System.EventArgs args) {
            if (!CanAct()) {
                return;
            }

            Mod.Static?.PaintJobService?.UndoPaintJob();
            Mod.Static?.RefreshPaintJobs();
        }

        private static void OnNextJob(object sender, System.EventArgs args) {
            CycleJob(1);
        }

        private static void OnPreviousJob(object sender, System.EventArgs args) {
            CycleJob(-1);
        }

        private static void OnOpenWorkbench(object sender, System.EventArgs args) {
            if (!CanAct()) {
                return;
            }

            Mod.Static?.OpenWorkbench();
        }

        private static void CycleJob(int offset) {
            if (!CanAct()) {
                return;
            }

            var service = Mod.Static?.PaintJobService;
            if (service == null) {
                return;
            }

            var job = service.CycleActiveJob(offset);

            MyAPIGateway.Utilities.ShowNotification(
                job != null
                    ? ModText.BC_PaintJob_ActiveJob.GetString(job.Name)
                    : ModText.BC_NoPaintJobsAvailable.GetString(),
                2000);

            Mod.Static?.RefreshPaintJobs(job);
        }

        /// <summary>
        /// Whether a world action should run at all. A key that is being typed into a field, or pressed while
        /// the chat is open, means the letter and not the paint job.
        /// </summary>
        private static bool CanAct() {
            if (MyAPIGateway.Gui == null || MyAPIGateway.Gui.ChatEntryVisible) {
                return false;
            }

            return HudMain.InputMode != HudInputMode.Full;
        }
    }
}
