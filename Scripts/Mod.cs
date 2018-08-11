using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using Sisk.BuildColors.Net;
using Sisk.BuildColors.Net.Messages;
using Sisk.BuildColors.Settings;
using Sisk.BuildColors.Settings.Models;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;
using Color = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors {
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class Mod : MySessionComponentBase {
        public const string NAME = "BuildColors";
        private const int COLOR_SEND_DELAY_IN_SECONDS = 300;
        private const string COLOR_SETS_FILE = "ColorSets.xml";
        private const ushort NETWORK_ID = 51500;
        private const string SETTINGS_FILE = "settings.xml";

        private readonly CommandHandler _commandHandler = new CommandHandler();
        private List<Vector3> _lastColorsSend;
        private DateTime _lasTimeColorSend;

        public Mod() {
            _commandHandler.Prefix = $"/{Acronym}";
            _commandHandler.Register(new Command { Name = "Save", Description = "Saves current build colors to set with given name.", Execute = SaveColorSet });
            _commandHandler.Register(new Command { Name = "Load", Description = "Load color set with given name.", Execute = LoadColorSet });
            _commandHandler.Register(new Command { Name = "List", Description = "Lists all available color sets.", Execute = ListColorSets });
        }

        /// <summary>
        ///     Mod name to acronym.
        /// </summary>
        public static string Acronym => string.Concat(NAME.Where(char.IsUpper));

        /// <summary>
        ///     Available color sets.
        /// </summary>
        public ColorSets ColorSets { get; private set; }

        /// <summary>
        ///     Network to handle syncing.
        /// </summary>
        public Network Network { get; private set; }

        /// <summary>
        ///     Mod settings
        /// </summary>
        public ModSettings Settings { get; private set; }

        public override void BeforeStart() {
            if (Network.IsClient) {
                Network.SendToServer(new RequestBuildColors { SteamId = MyAPIGateway.Session.LocalHumanPlayer.SteamUserId });
            }
        }

        /// <summary>
        ///     Initialize components.
        /// </summary>
        /// <param name="sessionComponent"></param>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {
            base.Init(sessionComponent);

            InitializeNetwork();
            Network.Register<BuildColorResponse>(OnBuildColorsReceived);
            if (Network.IsClient) {
                // todo: check if a manual color sync is needed, else remove AfterSimulation update method.
                //SetUpdateOrder(MyUpdateOrder.AfterSimulation);
            }

            if (Network.IsServer) {
                Network.Register<RequestBuildColors>(OnBuilderColorRequest);
            }

            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
        }

        /// <summary>
        ///     Load mod settings and create localizations.
        /// </summary>
        public override void LoadData() {
            if (MyAPIGateway.Multiplayer.MultiplayerActive && MyAPIGateway.Multiplayer.IsServer) {
                LoadPlayerColors();
            }

            LoadColorSets();
        }

        /// <summary>
        ///     Save mod settings.
        /// </summary>
        public override void SaveData() {
            if (MyAPIGateway.Multiplayer.MultiplayerActive && MyAPIGateway.Multiplayer.IsServer) {
                SavePlayerColors();
            }
        }

        public override void UpdateAfterSimulation() {
            var now = DateTime.UtcNow;
            if (now - _lasTimeColorSend > TimeSpan.FromSeconds(COLOR_SEND_DELAY_IN_SECONDS)) {
                var player = MyAPIGateway.Session.LocalHumanPlayer;
                if (player == null) {
                    return;
                }

                var colors = player.BuildColorSlots;
                if (!colors.SequenceEqual(_lastColorsSend) && !colors.SequenceEqual(player.DefaultBuildColorSlots)) {
                    _lastColorsSend = colors;
                    // todo: send build colors to server if colors never sync automatic.
                    //Network.SendToServer();
                }
            }
        }

        /// <summary>
        ///     Unregister events and stuff like that.
        /// </summary>
        protected override void UnloadData() {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            Network.Unregister<BuildColorResponse>(OnBuildColorsReceived);

            if (Network != null) {
                Network.Close();
                Network = null;
            }
        }

        /// <summary>
        ///     Initalize the network system.
        /// </summary>
        private void InitializeNetwork() {
            Network = new Network(NETWORK_ID);
        }

        /// <summary>
        ///     List available build color sets.
        /// </summary>
        /// <param name="arguments"></param>
        private void ListColorSets(string arguments) {
            MyAPIGateway.Utilities.ShowMessage(NAME, ColorSets.Any() ? string.Join(", ", ColorSets.Select(x => x.Name)) : "No color sets available.");
        }

        /// <summary>
        ///     Load build color set with given name.
        /// </summary>
        /// <param name="name">The name of the build color set.</param>
        private void LoadColorSet(string name) {
            var set = new ColorSet { Name = name };
            if (ColorSets.Contains(set)) {
                set = ColorSets.First(x => StringComparer.InvariantCultureIgnoreCase.Equals(x.Name, name));
                MyAPIGateway.Session.LocalHumanPlayer.BuildColorSlots = set.Colors.Select(x => (Vector3) x).ToList();
            } else {
                MyAPIGateway.Utilities.ShowMessage(NAME, $"No color set with name '{name}' found.");
            }
        }

        /// <summary>
        ///     Load color sets.
        /// </summary>
        private void LoadColorSets() {
            ColorSets colorSets = null;
            if (MyAPIGateway.Utilities.FileExistsInGlobalStorage(COLOR_SETS_FILE)) {
                using (var reader = MyAPIGateway.Utilities.ReadFileInGlobalStorage(COLOR_SETS_FILE)) {
                    colorSets = MyAPIGateway.Utilities.SerializeFromXML<ColorSets>(reader.ReadToEnd());
                }
            }

            if (colorSets != null) {
                if (colorSets.Version < ColorSets.VERSION) {
                    // todo: merge old and new color sets in future versions.
                }
            } else {
                colorSets = new ColorSets();
            }

            ColorSets = colorSets;
        }

        /// <summary>
        ///     Load player colors.
        /// </summary>
        private void LoadPlayerColors() {
            ModSettings settings = null;
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(SETTINGS_FILE, typeof(Mod))) {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(COLOR_SETS_FILE, typeof(Mod))) {
                    settings = MyAPIGateway.Utilities.SerializeFromXML<ModSettings>(reader.ReadToEnd());
                }
            }

            if (settings != null) {
                if (settings.Version < ColorSets.VERSION) {
                    // todo: merge old and new color sets in future versions.
                }
            } else {
                settings = new ModSettings();
            }

            Settings = settings;
        }

        private void OnBuildColorsReceived(ulong sender, BuildColorResponse message) {
            if (Network.IsClient) {
                var player = MyAPIGateway.Session.LocalHumanPlayer;
                if (player != null && message.BuildColors != null && message.BuildColors.Any()) {
                    player.BuildColorSlots = message.BuildColors;
                }
            }
        }

        private void OnBuilderColorRequest(ulong sender, RequestBuildColors message) {
            var data = Settings.Colors.FirstOrDefault(x => x.Id == message.SteamId).Colors;
            if (data == null) {
                return;
            }

            try {
                var colors = MyAPIGateway.Utilities.SerializeFromBinary<List<Vector3>>(data);
                var response = new BuildColorResponse {
                    BuildColors = colors,
                    SteamId = message.SteamId
                };

                Network.Send(response, sender);
            } catch (Exception exception) {
                throw new Exception("Unable to create response.", exception);
            }
        }

        private void OnMessageEntered(string messagetext, ref bool sendtoothers) {
            if (_commandHandler.TryHandle(messagetext.Trim())) {
                sendtoothers = false;
            }
        }

        /// <summary>
        ///     Saves or overrides a build color set with current build colors.
        /// </summary>
        /// <param name="name">The name of the build color set.</param>
        private void SaveColorSet(string name) {
            var set = new ColorSet { Name = name };
            if (ColorSets.Contains(set)) {
                ColorSets.Remove(set);
            }

            set.Colors = MyAPIGateway.Session.LocalHumanPlayer.BuildColorSlots.Select(x => (Color) x).ToArray();
            ColorSets.Add(set);

            SaveColorSets();
            MyAPIGateway.Utilities.ShowMessage(NAME, $"Color set '{name}' saved.");
        }

        /// <summary>
        ///     Save color sets.
        /// </summary>
        private void SaveColorSets() {
            using (var writer = MyAPIGateway.Utilities.WriteFileInGlobalStorage(COLOR_SETS_FILE)) {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(ColorSets));
            }
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