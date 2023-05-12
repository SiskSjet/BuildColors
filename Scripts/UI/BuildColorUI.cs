using RichHudFramework.Client;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;

namespace Sisk.BuildColors.UI {

    public sealed class BuildColorUI {
        private ScaledSpaceNode _scaledRoot;
        private BuildColorWindow _window;

        public BuildColorUI() { }

        public float AspectRatio { get; private set; }
        public float ResScale { get; private set; }
        public float ScreenHeight { get; private set; }
        public float ScreenWidth { get; private set; }
        private bool IsColorPickScreen => MyAPIGateway.Gui.ActiveGamePlayScreen == "ColorPick";

        public void Draw() {
            if (RichHudClient.Registered) {
                HudMain.EnableCursor = IsColorPickScreen;
                _window.Visible = IsColorPickScreen;
            }
        }

        public void Init(string modName) {
            RichHudClient.Init(modName, HudInit, ClientReset);
        }

        public void UpdateScreenScaling() {
            ScreenWidth = MyAPIGateway.Session.Camera.ViewportSize.X;
            ScreenHeight = MyAPIGateway.Session.Camera.ViewportSize.Y;
            AspectRatio = (ScreenWidth / ScreenHeight);
            ResScale = ScreenHeight / 1080f;
        }

        private void ClientReset() { }

        private void HudInit() {
            RichHudTerminal.Root.Enabled = true;
            _scaledRoot = new ScaledSpaceNode(HudMain.Root) {
                UpdateScaleFunc = () => ResScale
            };

            _window = new BuildColorWindow(_scaledRoot);
            UpdateScreenScaling();
        }
    }
}