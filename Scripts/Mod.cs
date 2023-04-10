using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.ColorSpace;
using Sisk.BuildColors.UI;
using Sisk.Utils.CommandHandler;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;
using Color = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors {

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class Mod : MySessionComponentBase {
        public const string NAME = "BuildColors";
        private const string COLOR_SETS_FILE = "ColorSets.xml";

        private readonly CommandHandler _commandHandler = new CommandHandler(NAME);
        private BuildColorUI _hud;

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
        /// </summary>
        public static Mod Static { get; private set; }

        /// <summary>
        ///     Available color sets.
        /// </summary>
        public ColorSets ColorSets { get; private set; }

        public override void Draw() {
            if (!MyAPIGateway.Utilities.IsDedicated) {
                _hud.Draw();
            }
        }

        /// <summary>
        ///     The static instance of this component.
        /// </summary>
        /// <param name="sessionComponent"></param>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {
            if (!MyAPIGateway.Utilities.IsDedicated) {
                _hud = new BuildColorUI();
                _hud?.Init(NAME);
            }
        }

        /// <summary>
        ///     Load build color set with given name.
        /// </summary>
        /// <param name="name">The name of the build color set.</param>
        public void LoadColorSet(string name) {
            var set = new ColorSet { Name = name };
            if (ColorSets.Contains(set)) {
                set = ColorSets.First(x => StringComparer.InvariantCultureIgnoreCase.Equals(x.Name, name));
                MyAPIGateway.Session.LocalHumanPlayer.BuildColorSlots = set.Colors.Select(x => (Vector3)x).ToList();
            } else {
                MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_NoColorSetFound.GetString(), name));
            }
        }

        /// <summary>
        ///     Load mod settings and create localizations.
        /// </summary>
        public override void LoadData() {
            CreateCommands();

            if (MyAPIGateway.Utilities.IsDedicated) {
                return;
            }

            LoadColorSets();
            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
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

            set.Colors = MyAPIGateway.Session.LocalHumanPlayer.BuildColorSlots.Select(x => (Color)x).ToArray();
            ColorSets.Add(set);

            SaveColorSets();
            MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_ColorSetSaved.GetString(), name));
        }

        public void SaveColorSet(ColorSet colorSet) {
            if (ColorSets.Contains(colorSet)) {
                ColorSets.Remove(colorSet);
            }

            ColorSets.Add(colorSet);
            SaveColorSets();
            MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_ColorSetSaved.GetString(), colorSet.Name));
        }

        /// <summary>
        ///     Unregister events and stuff like that.
        /// </summary>
        protected override void UnloadData() {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
        }

        /// <summary>
        ///     Create commands.
        /// </summary>
        private void CreateCommands() {
            _commandHandler.Prefix = $"/{Acronym}";
            _commandHandler.Register(new Command { Name = "Save", Description = ModText.BC_Description_Save.GetString(), Execute = SaveColorSet });
            _commandHandler.Register(new Command { Name = "Load", Description = ModText.BC_Description_Load.GetString(), Execute = LoadColorSet });
            _commandHandler.Register(new Command { Name = "Remove", Description = ModText.BC_Description_Remove.GetString(), Execute = RemoveColorSet });
            _commandHandler.Register(new Command { Name = "Generate", Description = ModText.BC_Description_Generate.GetString(), Execute = GenerateColorSet });
            _commandHandler.Register(new Command { Name = "List", Description = ModText.BC_Description_List.GetString(), Execute = ListColorSets });
            _commandHandler.Register(new Command { Name = "Help", Description = ModText.BC_Description_Help.GetString(), Execute = _commandHandler.ShowHelp });
        }

        private void GenerateColorSet(string arguments) {
            var generator = new ColorSchemeGenerator();
            var colorSet = generator.Generate();

            if (colorSet?.Length > 0) {
                MyAPIGateway.Session.LocalHumanPlayer.BuildColorSlots = colorSet.Select(x => ((VRageMath.Color)(Color)x).ToVector3()).ToList();
            }
        }

        /// <summary>
        ///     List available build color sets.
        /// </summary>
        /// <param name="arguments"></param>
        private void ListColorSets(string arguments) {
            MyAPIGateway.Utilities.ShowMessage(NAME, ColorSets.Any() ? string.Join(", ", ColorSets.Select(x => x.Name)) : ModText.BC_NoColorSetsAvailable.GetString());
        }

        private void LoadColorSets() {
            var colorSets = FileHandler.LoadColorSets(COLOR_SETS_FILE);

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

        private void SaveColorSets() {
            FileHandler.SaveColorSets(COLOR_SETS_FILE, ColorSets);
        }
    }
}