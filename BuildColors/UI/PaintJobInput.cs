using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Hotkeys that act on the world with none of the mod's UI on screen.
    /// </summary>
    internal static class PaintJobInput {
        public const string APPLY_BIND = "paintApply";
        public const string NEXT_JOB_BIND = "paintNextJob";
        public const string PREVIOUS_JOB_BIND = "paintPreviousJob";
        public const string UNDO_BIND = "paintUndo";

        /// <summary>
        /// Kept apart from the editing binds so the rebind page lists the two separately.
        /// </summary>
        private const string GROUP_NAME = "BuildColorsPaintJobs";

        /// <summary>
        /// Frames between rebuilds of the modifier cache, which picks up a rebind.
        /// </summary>
        private const int MODIFIER_REFRESH_INTERVAL = 120;

        private static readonly List<List<IControl>> _comboModifiers = new List<List<IControl>>();

        private static IBindGroup _binds;
        private static BindDefinition[] _defaultBinds;
        private static IBind _apply;
        private static IBind _undo;
        private static IBind _nextJob;
        private static IBind _previousJob;
        private static int _modifierRefreshCountdown;

        /// <summary>
        /// Registers the binds and starts listening.
        /// </summary>
        public static void Register() {
            EnsureBinds();
        }

        /// <summary>
        /// Keeps the game's own controls off the keys these binds are built from.
        /// </summary>
        public static void Update() {
            if (_binds == null) {
                return;
            }

            if (--_modifierRefreshCountdown <= 0) {
                RefreshModifiers();
            }

            if (CanAct() && IsComboInProgress()) {
                BindManager.RequestTempBlacklist(SeBlacklistModes.AllKeys);
            }
        }

        /// <summary>
        /// True while every modifier of at least one bind is held, meaning a combo is part way through.
        /// </summary>
        private static bool IsComboInProgress() {
            for (var i = 0; i < _comboModifiers.Count; i++) {
                var modifiers = _comboModifiers[i];
                var allHeld = true;

                for (var j = 0; j < modifiers.Count; j++) {
                    if (!modifiers[j].IsPressed) {
                        allHeld = false;
                        break;
                    }
                }

                if (allHeld) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Caches the controls each combo needs held before its last key.
        /// </summary>
        private static void RefreshModifiers() {
            _modifierRefreshCountdown = MODIFIER_REFRESH_INTERVAL;
            _comboModifiers.Clear();

            for (var i = 0; i < _binds.Count; i++) {
                var bind = _binds[i];

                for (var alias = 0; alias < bind.AliasCount; alias++) {
                    var combo = bind.GetConIDs(alias);

                    if (combo == null || combo.Count < 2) {
                        continue;
                    }

                    var modifiers = new List<IControl>(combo.Count - 1);

                    for (var control = 0; control < combo.Count - 1; control++) {
                        modifiers.Add(BindManager.GetControl(new ControlHandle(combo[control])));
                    }

                    _comboModifiers.Add(modifiers);
                }
            }
        }

        /// <summary>
        /// The keys a bind is currently set to, as the rebind page spells them.
        /// </summary>
        public static string DescribeBind(string bindName) {
            EnsureBinds();

            var bind = _binds?[bindName];
            var combo = bind?.GetCombo();

            if (combo == null || combo.Count == 0) {
                return ModText.BC_UI_Unbound.GetString();
            }

            var text = new StringBuilder();

            for (var i = 0; i < combo.Count; i++) {
                var control = BindManager.GetControl(combo[i]);

                if (i > 0) {
                    text.Append(" + ");
                }

                text.Append(control != null ? control.DisplayName : ModText.BC_UI_Unbound.GetString());
            }

            return text.ToString();
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
        /// Drops every handle taken from Rich HUD so the next init rebuilds them.
        /// </summary>
        public static void Reset() {
            Unsubscribe();

            _binds = null;
            _defaultBinds = null;
            _apply = null;
            _undo = null;
            _nextJob = null;
            _previousJob = null;
            _comboModifiers.Clear();
            _modifierRefreshCountdown = 0;
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
                { APPLY_BIND, RichHudControls.Alt, RichHudControls.P },
                { UNDO_BIND, RichHudControls.Alt, RichHudControls.Z },
                { NEXT_JOB_BIND, RichHudControls.Alt, RichHudControls.OemPeriod },
                { PREVIOUS_JOB_BIND, RichHudControls.Alt, RichHudControls.OemComma },
            });

            _defaultBinds = _binds.GetBindDefinitions();

            _apply = _binds[APPLY_BIND];
            _undo = _binds[UNDO_BIND];
            _nextJob = _binds[NEXT_JOB_BIND];
            _previousJob = _binds[PREVIOUS_JOB_BIND];

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
        }

        private static void Unsubscribe() {
            if (_apply == null) {
                return;
            }

            _apply.NewPressed -= OnApply;
            _undo.NewPressed -= OnUndo;
            _nextJob.NewPressed -= OnNextJob;
            _previousJob.NewPressed -= OnPreviousJob;
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
        /// Whether a world action should run at all.
        /// </summary>
        private static bool CanAct() {
            if (MyAPIGateway.Gui == null || MyAPIGateway.Gui.ChatEntryVisible) {
                return false;
            }

            return HudMain.InputMode != HudInputMode.Full;
        }
    }
}
