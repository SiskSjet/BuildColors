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
        /// Shares other players have sent this session.
        /// </summary>
        public ShareInbox Inbox { get; private set; }

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
            ShareNetwork.Register();

            if (MyAPIGateway.Utilities.IsDedicated) {
                return;
            }

            Inbox = new ShareInbox();
            ShareNetwork.Received += OnShareReceived;

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
        /// The given name when it is free, otherwise the first numbered copy of it that is.
        /// </summary>
        public string UniqueColorSetName(string name) {
            if (string.IsNullOrEmpty(name)) {
                name = ModText.BC_UI_ImportedSetName.GetString();
            }

            if (!HasColorSet(name)) {
                return name;
            }

            var candidate = ModText.BC_UI_CopyOfName.GetString(name);
            var index = 2;

            while (HasColorSet(candidate)) {
                candidate = string.Format("{0} {1}", ModText.BC_UI_CopyOfName.GetString(name), index);
                index++;
            }

            return candidate;
        }

        /// <summary>
        /// Rebuilds the share counts in the UI after the inbox changed.
        /// </summary>
        internal void RefreshShares() {
            _ui?.RefreshShares();
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
            ShareNetwork.Received -= OnShareReceived;
            ShareNetwork.Unregister();

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
            _commandHandler.Register(new Command { Name = "Share", Description = ModText.BC_Description_Share.GetString(), Execute = ShareColorSet });
            _commandHandler.Register(new Command { Name = "Shares", Description = ModText.BC_Description_Shares.GetString(), Execute = ListShares });
            _commandHandler.Register(new Command { Name = "Accept", Description = ModText.BC_Description_Accept.GetString(), Execute = AcceptShare });
            _commandHandler.Register(new Command { Name = "Decline", Description = ModText.BC_Description_Decline.GetString(), Execute = DeclineShare });
            _commandHandler.Register(new Command { Name = "Help", Description = ModText.BC_Description_Help.GetString(), Execute = _commandHandler.ShowHelp });

            PaintJobCommands.Register(_commandHandler);
        }

        /// <summary>
        /// Sends a color set to one player, or to everyone online when no player is named.
        /// </summary>
        private void ShareColorSet(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count < 1 || tokens.Count > 2) {
                MyAPIGateway.Utilities.ShowMessage(NAME, ModText.BC_Cmd_Usage_Share.GetString(Acronym));
                return;
            }

            var set = new ColorSet { Name = tokens[0] };

            if (ColorSets == null || !ColorSets.Contains(set)) {
                MyAPIGateway.Utilities.ShowMessage(NAME, string.Format(ModText.BC_NoColorSetFound.GetString(), tokens[0]));
                return;
            }

            set = ColorSets.First(x => StringComparer.InvariantCultureIgnoreCase.Equals(x.Name, tokens[0]));

            ShareService.Share(new SharePacket { Kind = ShareKind.ColorSet, ColorSet = set.Upgraded() }, tokens.Count == 2 ? tokens[1] : null);
        }

        private void ListShares(string arguments) {
            ShareService.ListInbox();
        }

        private void AcceptShare(string arguments) {
            ShareService.AcceptByReference((arguments ?? string.Empty).Trim());
        }

        private void DeclineShare(string arguments) {
            ShareService.DeclineByReference((arguments ?? string.Empty).Trim());
        }

        private void OnShareReceived(SharePacket packet) {
            ShareService.Receive(packet);
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