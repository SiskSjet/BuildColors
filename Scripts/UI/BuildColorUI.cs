using RichHudFramework.Client;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;

namespace Sisk.BuildColors.UI {

    public sealed class BuildColorUI {
        private BuildColorWindow _window;

        public BuildColorUI() { }

        private static bool IsColorPickScreen => MyAPIGateway.Gui.ActiveGamePlayScreen == "ColorPick";

        public void Draw() {
            if (RichHudClient.Registered) {
                HudMain.EnableCursor = IsColorPickScreen;
                _window.Visible = IsColorPickScreen;
            }
        }

        public void Init(string modName) {
            RichHudClient.Init(modName, HudInit, ClientReset);
        }

        private void ClientReset() { }

        private void HudInit() {
            RichHudTerminal.Root.Enabled = true;
            _window = new BuildColorWindow(HudMain.HighDpiRoot);
        }
    }
}