using RichHudFramework.Client;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Main UI controller for BuildColors. Manages the main window and UI visibility.
    /// </summary>
    public sealed class BuildColorUI {
        private MainWindow _mainWindow;

        public BuildColorUI() { }

        private bool IsColorPickScreen => MyAPIGateway.Gui.ActiveGamePlayScreen == "ColorPick";

        public void Draw() {
            if (RichHudClient.Registered) {
                HudMain.EnableCursor = IsColorPickScreen;
                if (_mainWindow != null) {
                    _mainWindow.Visible = IsColorPickScreen;
                }
            }
        }

        public void Init(string modName) {
            RichHudClient.Init(modName, HudInit, ClientReset);
        }

        /// <summary>
        /// Rich HUD tears its API down on reset, so every handle cached from it has to be dropped. The next
        /// HudInit rebuilds the window, the binds and the settings page.
        /// </summary>
        private void ClientReset() {
            _mainWindow = null;
            ReorderInput.Reset();
        }

        private void HudInit() {
            _mainWindow = new MainWindow(HudMain.HighDpiRoot);
            RegisterSettingsMenu();
        }

        /// <summary>
        /// Publishes the mod's binds in the Rich HUD terminal so they can be rebound. Aliases are exposed
        /// because every reorder bind carries a keyboard control plus a gamepad alternate.
        /// </summary>
        private void RegisterSettingsMenu() {
            var controls = new RebindPage { Name = "Controls" };
            controls.Add(ReorderInput.Binds, ReorderInput.DefaultBinds, true);

            RichHudTerminal.Root.Name = Mod.NAME;
            RichHudTerminal.Root.Enabled = true;
            RichHudTerminal.Root.Add(controls);
        }
    }
}