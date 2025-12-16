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

        private void ClientReset() { }

        private void HudInit() {
            _mainWindow = new MainWindow(HudMain.HighDpiRoot);
        }
    }
}