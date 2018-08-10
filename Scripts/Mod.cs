using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using Sisk.BuildColors.Settings;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace Sisk.BuildColors {
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class Mod : MySessionComponentBase {
        private const string SETTINGS_FILE = "settings.xml";

        /// <summary>
        ///     Save mod settings.
        /// </summary>
        // bug?: SaveData is called after the game save. You can use GetObjectBuilder if you need.
        public override void SaveData() {
            SavePlayerColors();
        }

        /// <summary>
        ///     Save player colors.
        /// </summary>
        private void SavePlayerColors() {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, x => !x.IsBot);

            var settings = new ModSettings {
                Colors = players.Select(x => new PlayerColors { Id = x.SteamUserId, Colors = MyAPIGateway.Utilities.SerializeToBinary(x.BuildColorSlots) }).ToArray()
            };

            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(SETTINGS_FILE, typeof(Mod))) {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(settings));
            }
        }
    }
}