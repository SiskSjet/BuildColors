using RichHudFramework.Client;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;

namespace Sisk.BuildColors.UI {

    public sealed class BuildColorHUD {
        public static readonly BuildColorHUD Instance = new BuildColorHUD();
        private BuildColorWindow _window;

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit! Don't remove it.
        static BuildColorHUD() { }

        private BuildColorHUD() {
        }

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

        private void ClientReset() {
        }

        private void HudInit() {
            RichHudTerminal.Root.Enabled = true;
            _window = new BuildColorWindow(HudMain.HighDpiRoot);
        }
    }
}