using RichHudFramework.Client;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using VRageMath;

namespace Sisk.BuildColors.UI {
    public static class UI {
        private static BuildColorMenu _buildColorMenu;
        private static bool IsColorPickScreen => MyAPIGateway.Gui.ActiveGamePlayScreen == "ColorPick";

        public static void Draw() {
            if (RichHudClient.Registered) {
                if (IsColorPickScreen || MyAPIGateway.Gui.ChatEntryVisible) {

                    _buildColorMenu.Scale = HudMain.ResScale;
                    _buildColorMenu.Visible = true;


                    //MyLog.Default.WriteLine($"SCREEN: {MyAPIGateway.Gui.GetCurrentScreen}");
                    //MyLog.Default.WriteLine($"SCREEN: {MyAPIGateway.Gui.ActiveGamePlayScreen}");
                    //MyLog.Default.WriteLine($"CURSOR: {MyAPIGateway.Gui.IsCursorVisible}");
                    //MyLog.Default.WriteLine($"SCREEN_SCALE: {HudMain.ResScale}");
                    //MyLog.Default.WriteLine($"SCREEN_WIDTH: {HudMain.ScreenWidth}");
                    //MyLog.Default.WriteLine($"SCREEN_HEIGHT: {HudMain.ScreenHeight}");
                    //MyLog.Default.WriteLine($"VISIBLE: {_buildColorMenu.Visible}");
                    //MyLog.Default.WriteLine($"WIDTH: {_buildColorMenu.Width}");
                    //MyLog.Default.WriteLine($"HEIGHT: {_buildColorMenu.Height}");
                    //MyLog.Default.WriteLine($"POSITION: {_buildColorMenu.Position}");
                    //MyLog.Default.WriteLine($"ALIGNMENT: {_buildColorMenu.ParentAlignment}");
                    //MyLog.Default.WriteLine($"SCALE: {_buildColorMenu.Scale}");
                } else {
                    _buildColorMenu.Visible = false;
                }
            }
        }

        public static void Init() {
            RichHudClient.Init(Mod.NAME, HudInit, ClientReset);
        }

        private static void ClientReset() {
            /* At this point, your client has been unregistered and all of 
            your framework members will stop working.

            This will be called in one of three cases:
            1) The game session is unloading.
            2) An unhandled exception has been thrown and caught on either the client
            or on master.
            3) RichHudClient.Reset() has been called manually.
            */
        }

        private static void HudInit() {
            _buildColorMenu = new BuildColorMenu();
        }
    }
}