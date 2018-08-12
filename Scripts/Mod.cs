﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Net.Messages;
using Sisk.BuildColors.Settings;
using Sisk.BuildColors.Settings.Models;
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

        private readonly CommandHandler _commandHandler = new CommandHandler();
        private readonly List<Vector3> _lastColorsSend = new List<Vector3>();
        private DateTime _lasTimeColorChecked = DateTime.UtcNow;

        public Mod() {
            BuildColorsText.CreateTranslations();
            _commandHandler.Prefix = $"/{Acronym}";
            _commandHandler.Register(new Command { Name = "Save", Description = BuildColorsText.Description_Save.String, Execute = SaveColorSet });
            _commandHandler.Register(new Command { Name = "Load", Description = BuildColorsText.Description_Load.String, Execute = LoadColorSet });
            _commandHandler.Register(new Command { Name = "Remove", Description = BuildColorsText.Description_Remove.String, Execute = RemoveColorSet });
            _commandHandler.Register(new Command { Name = "List", Description = BuildColorsText.Description_List.String, Execute = ListColorSets });
            _commandHandler.Register(new Command { Name = "Help", Description = BuildColorsText.Description_Help.String, Execute = _commandHandler.ShowHelp });
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
        ///     Initialize components.
        /// </summary>
        /// <param name="sessionComponent"></param>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {
            base.Init(sessionComponent);

            if (Network != null) {
                Network.Register<BuildColorMessage>(OnBuildColorsReceived);

                if (Network.IsClient) {
                    MyAPIGateway.Session.OnSessionReady += OnSessionReady;
                    SetUpdateOrder(MyUpdateOrder.AfterSimulation);
                }

                if (Network.IsServer) {
                    Network.Register<RequestBuildColors>(OnBuilderColorRequest);
                }
            }

            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
        }

        /// <summary>
        ///     Load mod settings and create localizations.
        /// </summary>
        public override void LoadData() {
            if (MyAPIGateway.Multiplayer.MultiplayerActive) {
                InitializeNetwork();
            }

            if (Network != null && Network.IsServer) {
                LoadPlayerColors();
            }

            LoadColorSets();
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
                if (Network.IsClient) {
                    SendColors();
                }

                Network.Unregister<BuildColorMessage>(OnBuildColorsReceived);

                if (Network.IsServer) {
                    Network.Unregister<RequestBuildColors>(OnBuilderColorRequest);
                }

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
            MyAPIGateway.Utilities.ShowMessage(NAME, ColorSets.Any() ? string.Join(", ", ColorSets.Select(x => x.Name)) : BuildColorsText.NoColorSetsAvailable.String);
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
                MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(BuildColorsText.NoColorSetFound.String, name));
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
                if (player != null && message.BuildColors != null && message.BuildColors.Any()) {
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
            if (!HasColorRequested) {
                var player = MyAPIGateway.Session.LocalHumanPlayer;
                if (player != null) {
                    Network.SendToServer(new RequestBuildColors { SteamId = player.SteamUserId });
                    HasColorRequested = true;
                    _lasTimeColorChecked = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        ///     Removes a Color Set with given name.
        /// </summary>
        /// <param name="name">The name of the color set.</param>
        private void RemoveColorSet(string name) {
            var set = new ColorSet { Name = name };
            if (!ColorSets.Contains(set)) {
                MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(BuildColorsText.NoColorSetFound.String, name));
                return;
            }

            ColorSets.Remove(set);

            SaveColorSets();
            MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(BuildColorsText.ColorSetRemoved.String, name));
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
            MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(BuildColorsText.ColorSetSaved.String, name));
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