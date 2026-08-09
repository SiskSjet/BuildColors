using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.BuildColors.UI;
using Sisk.BuildColors.Services;
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
        private const string PAINT_JOBS_FILE = "PaintJobs.xml";
        private const string SERVER_MEMORY_FILE = "BuildColorsServerMemory.xml";
        private readonly CommandHandler _commandHandler = new CommandHandler(NAME);
        private BuildColorUI _ui;

        /// <summary>
        /// Creates a new instance of this component.
        /// </summary>
        public Mod() {
            Static = this;
        }

        /// <summary>
        /// Mod name to acronym.
        /// </summary>
        public static string Acronym => string.Concat(NAME.Where(char.IsUpper));

        public static Mod Static { get; private set; }

        /// <summary>
        /// Available color sets.
        /// </summary>
        public ColorSets ColorSets { get; private set; }

        /// <summary>
        /// Stored paint jobs.
        /// </summary>
        public PaintJobSet PaintJobs { get; private set; }

        /// <summary>
        /// Runtime service for manipulating and applying paint jobs.
        /// </summary>
        public PaintJobService PaintJobService { get; private set; }

        /// <summary>
        /// Server memory.
        /// </summary>
        public ServerMemory ServerMemory { get; private set; }

        public override void Draw() {
            if (!MyAPIGateway.Utilities.IsDedicated) {
                _ui.Draw();
            }
        }

        /// <summary>
        /// The static instance of this component.
        /// </summary>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {
            if (!MyAPIGateway.Utilities.IsDedicated) {
                _ui = new BuildColorUI();
                _ui?.Init(NAME);
            }
        }

        /// <summary>
        /// Load build color set with given name.
        /// </summary>
        public void LoadColorSet(string name) {
            LoadColorSet(name, 0, ColorSet.SLOTS);
        }

        /// <summary>
        /// Loads a run of slots from a color set, leaving the rest of the palette as it is.
        /// </summary>
        public void LoadColorSet(string name, int startSlot, int slotCount) {
            var set = new ColorSet { Name = name };

            if (!ColorSets.Contains(set)) {
                MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_NoColorSetFound.GetString(), name));
                return;
            }

            set = ColorSets.First(x => StringComparer.InvariantCultureIgnoreCase.Equals(x.Name, name));
            ApplySlots(set.ToBuildColorSlots(), startSlot, slotCount);
        }

        /// <summary>
        /// Writes a run of slots into the palette, keeping every slot outside that run.
        /// </summary>
        public void ApplySlots(List<Vector3> slots, int startSlot = 0, int slotCount = ColorSet.SLOTS) {
            var player = MyAPIGateway.Session?.LocalHumanPlayer;

            if (player == null || slots == null || slots.Count == 0) {
                return;
            }

            var current = player.BuildColorSlots;
            var end = Math.Min(startSlot + slotCount, ColorSet.SLOTS);
            var applied = new List<Vector3>(ColorSet.SLOTS);

            for (var i = 0; i < ColorSet.SLOTS; i++) {
                if (i >= startSlot && i < end && i < slots.Count) {
                    applied.Add(slots[i]);
                } else if (current != null && i < current.Count) {
                    applied.Add(current[i]);
                } else {
                    applied.Add(i < slots.Count ? slots[i] : Vector3.Zero);
                }
            }

            player.BuildColorSlots = applied;
        }

        /// <summary>
        /// The palette the player is building with right now.
        /// </summary>
        public ColorMask[] GetCurrentPalette() {
            var slots = MyAPIGateway.Session?.LocalHumanPlayer?.BuildColorSlots;
            var masks = new ColorMask[ColorSet.SLOTS];

            for (var i = 0; i < ColorSet.SLOTS; i++) {
                masks[i] = slots != null && i < slots.Count ? (ColorMask)slots[i] : default(ColorMask);
            }

            return masks;
        }

        /// <summary>
        /// Load mod settings and create localizations.
        /// </summary>
        public override void LoadData() {
            CreateCommands();

            if (MyAPIGateway.Utilities.IsDedicated) {
                return;
            }

            LoadColorSets();
            LoadPaintJobs();
            PaintJobService = new PaintJobService(this);
            if (MyAPIGateway.Multiplayer.MultiplayerActive && !MyAPIGateway.Utilities.IsDedicated) {
                LoadServerColor();
                MyAPIGateway.Session.OnSessionReady += OnSessionReady;
            }

            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
        }

        /// <summary>
        /// Removes a Color Set with given name.
        /// </summary>
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
        /// Saves or overrides a build color set with current build colors.
        /// </summary>
        public void SaveColorSet(string name) {
            SaveColorSet(new ColorSet(name, GetCurrentPalette()));
        }

        public void SaveColorSet(ColorSet colorSet) {
            var set = colorSet.Upgraded();

            if (ColorSets.Contains(set)) {
                var existing = ColorSets.First(x => StringComparer.InvariantCultureIgnoreCase.Equals(x.Name, set.Name));

                if (set.CreatedTicks == 0L) {
                    set.CreatedTicks = existing.CreatedTicks;
                }

                ColorSets.Remove(set);
            }

            if (set.CreatedTicks == 0L) {
                set.CreatedTicks = DateTime.UtcNow.Ticks;
            }

            ColorSets.Add(set);
            SaveColorSets();
            MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_ColorSetSaved.GetString(), set.Name));
        }

        /// <summary>
        /// True when a set of this name already exists, so the UI can say what saving will replace.
        /// </summary>
        public bool HasColorSet(string name) {
            return ColorSets != null && ColorSets.Contains(new ColorSet { Name = name });
        }

        /// <summary>
        /// Stores a set read from a share code, under a name that is not taken.
        /// </summary>
        public bool ImportColorSet(string code, out string name) {
            ColorSet imported;
            name = null;

            if (!ColorSetCode.TryDecode(code, out imported)) {
                return false;
            }

            if (string.IsNullOrEmpty(imported.Name)) {
                imported = imported.WithName(ModText.BC_UI_ImportedSetName.GetString());
            }

            while (HasColorSet(imported.Name)) {
                imported = imported.WithName(ModText.BC_UI_CopyOfName.GetString(imported.Name));
            }

            name = imported.Name;
            SaveColorSet(imported);

            return true;
        }

        /// <summary>
        /// Pulls the paint job list of the UI back in line after a console command changed it.
        /// </summary>
        internal void RefreshPaintJobs(PaintJob jobToSelect = null) {
            _ui?.RefreshPaintJobs(jobToSelect);
        }

        internal void SavePaintJobs() {
            if (PaintJobs != null) {
                FileHandler.Save(PAINT_JOBS_FILE, PaintJobs);
            }
        }

        /// <summary>
        /// Unregister events and stuff like that.
        /// </summary>
        protected override void UnloadData() {
            if (MyAPIGateway.Multiplayer.MultiplayerActive && !MyAPIGateway.Utilities.IsDedicated) {
                SaveServerMemory();
                MyAPIGateway.Session.OnSessionReady -= OnSessionReady;
            }
            SavePaintJobs();
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            Static = null;
        }

        /// <summary>
        /// Create commands.
        /// </summary>
        private void CreateCommands() {
            _commandHandler.Prefix = $"/{Acronym}";
            _commandHandler.Register(new Command { Name = "Save", Description = ModText.BC_Description_Save.GetString(), Execute = SaveColorSet });
            _commandHandler.Register(new Command { Name = "Load", Description = ModText.BC_Description_Load.GetString(), Execute = LoadColorSet });
            _commandHandler.Register(new Command { Name = "Remove", Description = ModText.BC_Description_Remove.GetString(), Execute = RemoveColorSet });
            _commandHandler.Register(new Command { Name = "Generate", Description = ModText.BC_Description_Generate.GetString(), Execute = GenerateColorSet });
            _commandHandler.Register(new Command { Name = "List", Description = ModText.BC_Description_List.GetString(), Execute = ListColorSets });
            _commandHandler.Register(new Command { Name = "Export", Description = ModText.BC_Description_Export.GetString(), Execute = ExportColorSet });
            _commandHandler.Register(new Command { Name = "Import", Description = ModText.BC_Description_Import.GetString(), Execute = ImportColorSet });
            _commandHandler.Register(new Command { Name = "Help", Description = ModText.BC_Description_Help.GetString(), Execute = _commandHandler.ShowHelp });

            PaintJobCommands.Register(_commandHandler);
        }

        private void ExportColorSet(string arguments) {
            var name = (arguments ?? string.Empty).Trim().Trim('"');
            var set = new ColorSet { Name = name };

            if (!ColorSets.Contains(set)) {
                MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_NoColorSetFound.GetString(), name));
                return;
            }

            set = ColorSets.First(x => StringComparer.InvariantCultureIgnoreCase.Equals(x.Name, name));

            MyAPIGateway.Utilities.ShowMessage(NAME, ColorSetCode.Encode(set));
        }

        private void ImportColorSet(string arguments) {
            string name;

            if (ImportColorSet((arguments ?? string.Empty).Trim(), out name)) {
                MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_ColorSetImported.GetString(), name));
                return;
            }

            MyAPIGateway.Utilities.ShowMessage(NAME, ModText.BC_InvalidColorSetCode.GetString());
        }

        /// <summary>
        /// Rolls a palette straight into the build colors.
        /// </summary>
        private void GenerateColorSet(string arguments) {
            var generator = new ColorSchemeGenerator();
            var options = new ColorSchemeGenerator.Options();

            foreach (var token in (arguments ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
                ColorSchemeGenerator.Scheme scheme;
                ColorSchemeGenerator.Preset preset;

                if (Enum.TryParse(token, true, out scheme) && Enum.IsDefined(typeof(ColorSchemeGenerator.Scheme), scheme)) {
                    options.Scheme = scheme;
                } else if (Enum.TryParse(token, true, out preset) && Enum.IsDefined(typeof(ColorSchemeGenerator.Preset), preset)) {
                    options.Preset = preset;
                }
            }

            var result = generator.Generate(options);

            if (result.Masks.Length > 0) {
                ApplySlots(result.ToSlots());
                MyAPIGateway.Utilities.ShowMessage(NAME, ModText.BC_ColorSetGenerated.GetString());
            }
        }

        /// <summary>
        /// List available build color sets.
        /// </summary>
        private void ListColorSets(string arguments) {
            MyAPIGateway.Utilities.ShowMessage(NAME, ColorSets.Any() ? string.Join(", ", ColorSets.Select(x => x.Name)) : ModText.BC_NoColorSetsAvailable.GetString());
        }

        private void LoadColorSets() {
            var colorSets = FileHandler.Load<ColorSets>(COLOR_SETS_FILE) ?? new ColorSets();

            colorSets.Upgrade();
            ColorSets = colorSets;
        }

        private void LoadPaintJobs() {
            var jobs = FileHandler.Load<PaintJobSet>(PAINT_JOBS_FILE);

            PaintJobs = jobs ?? new PaintJobSet();
        }

        private void LoadServerColor() {
            var serverMemory = FileHandler.Load<ServerMemory>(SERVER_MEMORY_FILE);

            if (serverMemory != null) {
                if (serverMemory.Version < ColorSets.VERSION) {
                }
            } else {
                serverMemory = new ServerMemory();
            }

            ServerMemory = serverMemory;
        }

        private void OnMessageEntered(string messagetext, ref bool sendtoothers) {
            if (_commandHandler.TryHandle(messagetext.Trim())) {
                sendtoothers = false;
            }
        }

        private void OnSessionReady() {
            if (ServerMemory?.ServerEntries == null || MyAPIGateway.Session?.LocalHumanPlayer == null) {
                return;
            }

            var name = MyAPIGateway.Session.Name;

            if (ServerMemory.ServerEntries.Any(x => x.Id == name)) {
                var entry = ServerMemory.ServerEntries.First(x => x.Id == name);
                ApplySlots(entry.ToBuildColorSlots());
            }
        }

        private void SaveColorSets() {
            FileHandler.Save(COLOR_SETS_FILE, ColorSets);
        }

        private void SaveServerMemory() {
            var slots = MyAPIGateway.Session?.LocalHumanPlayer?.BuildColorSlots;

            if (ServerMemory?.ServerEntries == null || slots == null) {
                return;
            }

            var name = MyAPIGateway.Session.Name;
            var entry = ServerEntry.FromSlots(name, slots);

            ServerMemory.ServerEntries.RemoveWhere(x => x.Id == name);
            ServerMemory.ServerEntries.Add(entry);

            ServerMemory.Version = ServerMemory.VERSION;
            FileHandler.Save(SERVER_MEMORY_FILE, ServerMemory);
        }
    }
}