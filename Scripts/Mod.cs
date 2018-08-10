using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
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
        private const string COLOR_SETS_FILE = "ColorSets.xml";
        private const string SETTINGS_FILE = "settings.xml";

        private readonly CommandHandler _commandHandler = new CommandHandler();

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
        ///     Initialize components.
        /// </summary>
        /// <param name="sessionComponent"></param>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {
            base.Init(sessionComponent);

            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
        }

        /// <summary>
        ///     Load mod settings and create localizations.
        /// </summary>
        public override void LoadData() {
            LoadColorSets();
        }

        /// <summary>
        ///     Save mod settings.
        /// </summary>
        public override void SaveData() {
            SavePlayerColors();
        }

        /// <summary>
        ///     Unregister events and stuff like that.
        /// </summary>
        protected override void UnloadData() {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
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