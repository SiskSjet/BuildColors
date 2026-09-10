using RichHudFramework.UI;
using Sisk.BuildColors.Settings;
using System;
using System.Collections.Generic;
using VRage.Utils;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Saves the combos of the mod's bind groups. Rich HUD keeps no config for client binds, so a
    /// rebind or a removed bind would be back to its default on the next load.
    /// </summary>
    internal static class HotkeyStore {
        private const string FILE_NAME = "Hotkeys.xml";

        private static HotkeySettings _settings;
        private static bool _hasChanges;

        /// <summary>
        /// Puts the stored combos of a group over the ones it was registered with.
        /// </summary>
        public static void Apply(string groupName, IBindGroup group) {
            if (group == null) {
                return;
            }

            var stored = FindGroup(groupName);

            if (stored == null || stored.Binds == null || stored.Binds.Length == 0) {
                return;
            }

            if (!group.TryLoadBindData(stored.Binds)) {
                MyLog.Default.Warning($"[{Mod.NAME}] Stored hotkeys of group '{groupName}' could not be applied, defaults kept.");
            }
        }

        /// <summary>
        /// Takes the combos a group is set to right now.
        /// </summary>
        public static void Capture(string groupName, IBindGroup group) {
            if (group == null) {
                return;
            }

            var definitions = group.GetBindDefinitions();

            if (definitions == null || definitions.Length == 0) {
                return;
            }

            var stored = FindGroup(groupName);

            if (stored == null) {
                stored = new HotkeyGroup { Name = groupName };
                Settings.Groups.Add(stored);
            } else if (AreSame(stored.Binds, definitions)) {
                return;
            }

            stored.Binds = definitions;
            _hasChanges = true;
        }

        /// <summary>
        /// Writes the file if anything captured differs from what it holds.
        /// </summary>
        public static void Save() {
            if (!_hasChanges) {
                return;
            }

            FileHandler.Save(FILE_NAME, Settings);
            _hasChanges = false;
        }

        private static HotkeySettings Settings {
            get {
                if (_settings == null) {
                    _settings = FileHandler.Load<HotkeySettings>(FILE_NAME) ?? new HotkeySettings();

                    if (_settings.Groups == null) {
                        _settings.Groups = new List<HotkeyGroup>();
                    }
                }

                return _settings;
            }
        }

        private static HotkeyGroup FindGroup(string name) {
            var groups = Settings.Groups;

            for (var i = 0; i < groups.Count; i++) {
                if (string.Equals(groups[i].Name, name, StringComparison.OrdinalIgnoreCase)) {
                    return groups[i];
                }
            }

            return null;
        }

        private static bool AreSame(BindDefinition[] stored, BindDefinition[] current) {
            if (stored == null || current == null || stored.Length != current.Length) {
                return false;
            }

            for (var i = 0; i < stored.Length; i++) {
                var same = string.Equals(stored[i].name, current[i].name, StringComparison.OrdinalIgnoreCase)
                    && AreSame(stored[i].controlNames, current[i].controlNames)
                    && AreSame(stored[i].aliases, current[i].aliases);

                if (!same) {
                    return false;
                }
            }

            return true;
        }

        private static bool AreSame(BindAliasDefinition[] stored, BindAliasDefinition[] current) {
            var storedCount = stored != null ? stored.Length : 0;
            var currentCount = current != null ? current.Length : 0;

            if (storedCount != currentCount) {
                return false;
            }

            for (var i = 0; i < storedCount; i++) {
                if (!AreSame(stored[i].controlNames, current[i].controlNames)) {
                    return false;
                }
            }

            return true;
        }

        private static bool AreSame(string[] stored, string[] current) {
            var storedCount = stored != null ? stored.Length : 0;
            var currentCount = current != null ? current.Length : 0;

            if (storedCount != currentCount) {
                return false;
            }

            for (var i = 0; i < storedCount; i++) {
                if (!string.Equals(stored[i], current[i], StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }
            }

            return true;
        }
    }
}
