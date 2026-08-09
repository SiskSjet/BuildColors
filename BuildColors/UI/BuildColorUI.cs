using RichHudFramework.Client;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Main UI controller for BuildColors.
    /// </summary>
    public sealed class BuildColorUI {
        private MainWindow _mainWindow;

        public BuildColorUI() { }

        private bool IsColorPickScreen => MyAPIGateway.Gui.ActiveGamePlayScreen == "ColorPick";

        public void Draw() {
            if (!RichHudClient.Registered) {
                return;
            }

            PaintJobInput.Update();

            if (_mainWindow == null) {
                return;
            }

            var pickScreen = IsColorPickScreen;
            var hasDialogs = _mainWindow.HasOpenDialogs;

            HudMain.EnableCursor = pickScreen || hasDialogs;

            _mainWindow.Visible = pickScreen || hasDialogs;
            _mainWindow.PanelVisible = pickScreen;

            if (pickScreen) {
                _mainWindow.RebuildIfScreenChanged();
            }
        }

        public void Init(string modName) {
            RichHudClient.Init(modName, HudInit, ClientReset);
        }

        /// <summary>
        /// Hands any control the binds took over back to the game before the mod goes away.
        /// </summary>
        public void Close() {
            ClientReset();
        }

        /// <summary>
        /// Rebuilds the paint job list.
        /// </summary>
        public void RefreshPaintJobs(PaintJob jobToSelect = null) {
            _mainWindow?.RefreshPaintJobs(jobToSelect);
        }

        /// <summary>
        /// Updates the waiting share counts.
        /// </summary>
        public void RefreshShares() {
            _mainWindow?.RefreshShares();
        }

        /// <summary>
        /// Rich HUD tears its API down on reset, so every handle cached from it has to be dropped.
        /// </summary>
        private void ClientReset() {
            _mainWindow = null;
            ReorderInput.Reset();
            PaintJobInput.Reset();
        }

        private void HudInit() {
            _mainWindow = new MainWindow(HudMain.HighDpiRoot);
            RegisterSettingsMenu();

            PaintJobInput.Register();
        }

        /// <summary>
        /// Publishes the mod's binds in the Rich HUD terminal so they can be rebound.
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
