using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Net.Messages;
using Sisk.BuildColors.Settings;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.UI;
using Sisk.Utils.CommandHandler;
using Sisk.Utils.Localization.Extensions;
using Sisk.Utils.Net;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;
using Color = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors {
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class Mod : MySessionComponentBase {
        public const string NAME = "BuildColors";
        private const int COLOR_CHECK_IN_SECONDS = 30;
        private const string COLOR_SETS_FILE = "ColorSets.xml";
        private const ushort NETWORK_ID = 51500;
        private const string SETTINGS_FILE = "settings.xml";

        private readonly CommandHandler _commandHandler = new CommandHandler(NAME);
        private readonly List<Vector3> _lastColorsSend = new List<Vector3>();
        private DateTime _lasTimeColorChecked = DateTime.UtcNow;
        private BuildColorHUD _hud;

        /// <summary>
        ///     Creates a new instance of this component.
        /// </summary>
        public Mod() {
            Static = this;
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
        ///     Indicates if colors are requested.
        /// </summary>
        private bool HasColorRequested { get; set; }

        /// <summary>
        ///     Network to handle syncing.
        /// </summary>
        public Network Network { get; private set; }

        /// <summary>
        ///     Mod settings
        /// </summary>
        public ModSettings Settings { get; private set; }

        /// <summary>
        /// </summary>
        public static Mod Static { get; private set; }

        public override void Draw() {
            if (!MyAPIGateway.Utilities.IsDedicated) {
                //UI.UI.Draw();
                _hud.Draw();
            }
        }

        /// <summary>
        ///     The static instance of this component.
        /// </summary>
        /// <param name="sessionComponent"></param>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {
            if (!MyAPIGateway.Utilities.IsDedicated) {
                //UI.UI.Init();
                _hud = UI.BuildColorHUD.Instance;
                _hud?.Init(NAME);
            }
        }

        /// <summary>
        ///     Load mod settings and create localizations.
        /// </summary>
        public override void LoadData() {
            CreateCommands();

            if (MyAPIGateway.Multiplayer.MultiplayerActive) {
                InitializeNetwork();

                if (Network != null) {
                    Network.Register<BuildColorMessage>(OnBuildColorsReceived);
                    Network.Register<RequestBuildColors>(OnBuilderColorRequest);

                    if (Network.IsServer) {
                        LoadPlayerColors();

                        if (Network.IsDedicated) {
                            return;
                        }
                    }

                    MyAPIGateway.Session.OnSessionReady += OnSessionReady;
                }
            }

            LoadColorSets();
            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
        }

        /// <summary>
        ///     Save mod settings.
        /// </summary>
        public override void SaveData() {
            if (Network != null && Network.IsServer) {
                SavePlayerColors();
            }
        }

        /// <summary>
        ///     Check for color changes if client and send them.
        /// </summary>
        public override void UpdateAfterSimulation() {
            if (Network == null || !Network.IsClient) {
                return;
            }

            var now = DateTime.UtcNow;
            if (now - _lasTimeColorChecked > TimeSpan.FromSeconds(COLOR_CHECK_IN_SECONDS)) {
                SendColors();
                _lasTimeColorChecked = now;
            }
        }

        /// <summary>
        ///     Unregister events and stuff like that.
        /// </summary>
        protected override void UnloadData() {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;

            if (Network != null) {
                MyAPIGateway.Session.OnSessionReady -= OnSessionReady;
                Network.Unregister<BuildColorMessage>(OnBuildColorsReceived);

                if (Network.IsServer) {
                    Network.Unregister<RequestBuildColors>(OnBuilderColorRequest);
                }

                Network.Close();
                Network = null;
            }
        }

        /// <summary>
        ///     Create commands.
        /// </summary>
        private void CreateCommands() {
            _commandHandler.Prefix = $"/{Acronym}";
            _commandHandler.Register(new Command { Name = "Save", Description = ModText.BC_Description_Save.GetString(), Execute = SaveColorSet });
            _commandHandler.Register(new Command { Name = "Load", Description = ModText.BC_Description_Load.GetString(), Execute = LoadColorSet });
            _commandHandler.Register(new Command { Name = "Remove", Description = ModText.BC_Description_Remove.GetString(), Execute = RemoveColorSet });
            _commandHandler.Register(new Command { Name = "List", Description = ModText.BC_Description_List.GetString(), Execute = ListColorSets });
            _commandHandler.Register(new Command { Name = "Help", Description = ModText.BC_Description_Help.GetString(), Execute = _commandHandler.ShowHelp });
        }

        /// <summary>
        ///     Initialize the network system.
        /// </summary>
        private void InitializeNetwork() {
            Network = new Network(NETWORK_ID);
        }

        /// <summary>
        ///     List available build color sets.
        /// </summary>
        /// <param name="arguments"></param>
        private void ListColorSets(string arguments) {
            MyAPIGateway.Utilities.ShowMessage(NAME, ColorSets.Any() ? string.Join(", ", ColorSets.Select(x => x.Name)) : ModText.BC_NoColorSetsAvailable.GetString());
        }

        /// <summary>
        ///     Load build color set with given name.
        /// </summary>
        /// <param name="name">The name of the build color set.</param>
        public void LoadColorSet(string name) {
            var set = new ColorSet { Name = name };
            if (ColorSets.Contains(set)) {
                set = ColorSets.First(x => StringComparer.InvariantCultureIgnoreCase.Equals(x.Name, name));
                MyAPIGateway.Session.LocalHumanPlayer.BuildColorSlots = set.Colors.Select(x => (Vector3) x).ToList();
            } else {
                MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_NoColorSetFound.GetString(), name));
            }
        }

        /// <summary>
        ///     Load color sets.
        /// </summary>
        private void LoadColorSets() {
            ColorSets colorSets = null;
            if (MyAPIGateway.Utilities.FileExistsInGlobalStorage(COLOR_SETS_FILE)) {
                try {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInGlobalStorage(COLOR_SETS_FILE)) {
                        colorSets = MyAPIGateway.Utilities.SerializeFromXML<ColorSets>(reader.ReadToEnd());
                    }
                } catch (Exception exception) {
                    // ignored
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
                try {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(SETTINGS_FILE, typeof(Mod))) {
                        settings = MyAPIGateway.Utilities.SerializeFromXML<ModSettings>(reader.ReadToEnd());
                    }
                } catch (Exception exception) {
                    // ignored
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

        private void OnBuildColorsReceived(ulong sender, BuildColorMessage message) {
            if (Network.IsClient) {
                var player = MyAPIGateway.Session.LocalHumanPlayer;
                if (player != null && player.SteamUserId == message.SteamId && message.BuildColors != null && message.BuildColors.Any()) {
                    player.BuildColorSlots = message.BuildColors;
                    _lastColorsSend.Clear();
                    _lastColorsSend.AddRange(player.BuildColorSlots);
                }
            } else if (Network.IsServer) {
                var playerColor = new PlayerColors { Id = message.SteamId, Colors = MyAPIGateway.Utilities.SerializeToBinary(message.BuildColors) };
                if (Settings.Colors.Contains(playerColor)) {
                    Settings.Colors.Remove(playerColor);
                }

                Settings.Colors.Add(playerColor);
            }
        }

        private void OnBuilderColorRequest(ulong sender, RequestBuildColors message) {
            var data = Settings.Colors.FirstOrDefault(x => x.Id == message.SteamId).Colors;
            if (data == null) {
                return;
            }

            try {
                var colors = MyAPIGateway.Utilities.SerializeFromBinary<List<Vector3>>(data);
                var response = new BuildColorMessage {
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

        private void OnSessionReady() {
            MyAPIGateway.Session.OnSessionReady -= OnSessionReady;
            if (!HasColorRequested) {
                var player = MyAPIGateway.Session.LocalHumanPlayer;
                if (player != null) {
                    Network.SendToServer(new RequestBuildColors { SteamId = player.SteamUserId });

                    HasColorRequested = true;
                    _lasTimeColorChecked = DateTime.UtcNow;
                }
            }

            SetUpdateOrder(MyUpdateOrder.AfterSimulation);
        }

        /// <summary>
        ///     Removes a Color Set with given name.
        /// </summary>
        /// <param name="name">The name of the color set.</param>
        public void RemoveColorSet(string name) {
            var set = new ColorSet { Name = name };
            if (!ColorSets.Contains(set)) {
                MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_NoColorSetFound.GetString(), name));
                return;
            }

            ColorSets.Remove(set);

            SaveColorSets();
            MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_ColorSetRemoved.GetString(), name));
        }

        /// <summary>
        ///     Saves or overrides a build color set with current build colors.
        /// </summary>
        /// <param name="name">The name of the build color set.</param>
        public void SaveColorSet(string name) {
            var set = new ColorSet { Name = name };
            if (ColorSets.Contains(set)) {
                ColorSets.Remove(set);
            }

            set.Colors = MyAPIGateway.Session.LocalHumanPlayer.BuildColorSlots.Select(x => (Color) x).ToArray();
            ColorSets.Add(set);

            SaveColorSets();
            MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_ColorSetSaved.GetString(), name));
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
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(SETTINGS_FILE, typeof(Mod))) {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(Settings));
            }
        }

        /// <summary>
        ///     Send colors to server if not default colors.
        /// </summary>
        private void SendColors() {
            var player = MyAPIGateway.Session.LocalHumanPlayer;
            if (player == null) {
                return;
            }

            var colors = player.BuildColorSlots;
            if (!colors.SequenceEqual(_lastColorsSend)) {
                var message = new BuildColorMessage {
                    BuildColors = colors,
                    SteamId = player.SteamUserId
                };

                Network.SendToServer(message);

                _lastColorsSend.Clear();
                _lastColorsSend.AddRange(colors);
                _lasTimeColorChecked = DateTime.UtcNow;
            }
        }
    }
}