using RichHudFramework.Client;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Main UI controller for BuildColors. Manages the main window and UI visibility.
    /// </summary>
    public sealed class BuildColorUI {
        private MainWindow _mainWindow;
        private PaintJobWorkbench _workbench;

        public BuildColorUI() { }

        private bool IsColorPickScreen => MyAPIGateway.Gui.ActiveGamePlayScreen == "ColorPick";

        public void Draw() {
            if (!RichHudClient.Registered) {
                return;
            }

            var pickScreen = IsColorPickScreen;
            var hasDialogs = _mainWindow != null && _mainWindow.HasOpenDialogs;

            // The workbench is opened by a hotkey and lives outside the colour picker, so the cursor and the
            // container follow whether anything is actually on screen rather than which screen is up.
            HudMain.EnableCursor = pickScreen || hasDialogs;

            if (_mainWindow != null) {
                _mainWindow.Visible = pickScreen || hasDialogs;
                _mainWindow.PanelsVisible = pickScreen;
            }
        }

        /// <summary>
        /// Opens the paint job workbench, or brings it forward when it is already up.
        /// </summary>
        public void OpenWorkbench() {
            if (_mainWindow == null || _workbench != null) {
                return;
            }

            _workbench = new PaintJobWorkbench();
            _workbench.Closed += (s, e) => _workbench = null;

            _mainWindow.ShowDialog(_workbench);
        }

        public void Init(string modName) {
            RichHudClient.Init(modName, HudInit, ClientReset);
        }

        /// <summary>
        /// Rebuilds the paint job list. The window only exists once Rich HUD has handed one out, so before
        /// that there is nothing to refresh and the list is built from the current jobs anyway.
        /// </summary>
        public void RefreshPaintJobs(PaintJob jobToSelect = null) {
            _mainWindow?.PaintJobPanel?.Refresh(jobToSelect);
        }

        /// <summary>
        /// Rich HUD tears its API down on reset, so every handle cached from it has to be dropped. The next
        /// HudInit rebuilds the window, the binds and the settings page.
        /// </summary>
        private void ClientReset() {
            _mainWindow = null;
            _workbench = null;
            ReorderInput.Reset();
            PaintJobInput.Reset();
        }

        private void HudInit() {
            _mainWindow = new MainWindow(HudMain.HighDpiRoot);
            RegisterSettingsMenu();

            // Registered here rather than lazily on first use: these hotkeys are meant to work with none of
            // this mod's UI on screen, so nothing else would ever trigger their registration.
            PaintJobInput.Register();
        }

        /// <summary>
        /// Publishes the mod's binds in the Rich HUD terminal so they can be rebound. Aliases are exposed
        /// because every reorder bind carries a keyboard control plus a gamepad alternate.
        /// </summary>
        private void RegisterSettingsMenu() {
            var controls = new RebindPage { Name = ModText.BC_UI_Controls.GetString() };
            controls.Add(PaintJobInput.Binds, PaintJobInput.DefaultBinds, true);
            controls.Add(ReorderInput.Binds, ReorderInput.DefaultBinds, true);

            RichHudTerminal.Root.Name = Mod.NAME;
            RichHudTerminal.Root.Enabled = true;
            RichHudTerminal.Root.Add(controls);
        }
    }
}