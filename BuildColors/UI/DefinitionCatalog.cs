using Sandbox.Definitions;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;
using System.Collections.Generic;
using System.Linq;
using RichHudFramework.UI;
using VRage.Game;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Lazily built, cached lists of block definitions and armor skins used to populate the paint job dialogs.
    /// </summary>
    internal static class DefinitionCatalog {
        private static List<BlockDefinitionOption> _blockDefinitions;
        private static List<SkinOption> _skins;
        private static bool _skinsFilteredByOwnership;

        public static IReadOnlyList<BlockDefinitionOption> BlockDefinitions {
            get {
                if (_blockDefinitions == null) {
                    _blockDefinitions = BuildBlockDefinitions();
                }

                return _blockDefinitions;
            }
        }

        public static IReadOnlyList<SkinOption> Skins {
            get {
                if (_skins == null || !_skinsFilteredByOwnership) {
                    _skins = BuildSkins();
                }

                return _skins;
            }
        }

        /// <summary>
        /// Fills a skin dropdown with the catalog and applies each entry's icon.
        /// </summary>
        public static Dropdown<SkinListEntry, SkinOption> CreateSkinDropdown(float height) {
            var dropdown = new Dropdown<SkinListEntry, SkinOption>() {
                DimAlignment = DimAlignments.Width,
                Height = height,
            };

            ControlFactory.StyleDropdown(dropdown);

            foreach (var option in Skins) {
                dropdown.Add(option.DisplayName, option);
            }

            for (var i = 0; i < dropdown.EntryList.Count; i++) {
                dropdown.EntryList[i].Element.SetIcon(dropdown.EntryList[i].AssocMember.IconMaterial);
            }

            return dropdown;
        }

        public static int IndexOfSkin(string skinId) {
            var skins = Skins;
            var normalized = skinId ?? string.Empty;

            for (var i = 0; i < skins.Count; i++) {
                if (string.Equals(skins[i].SkinId, normalized, System.StringComparison.OrdinalIgnoreCase)) {
                    return i;
                }
            }

            return -1;
        }

        private static List<BlockDefinitionOption> BuildBlockDefinitions() {
            var options = MyDefinitionManager.Static
                .GetDefinitionsOfType<MyCubeBlockDefinition>()
                .Where(definition => definition.Public)
                .Select(definition => new BlockDefinitionOption {
                    TypeId = definition.Id.TypeId.ToString(),
                    SubtypeId = definition.Id.SubtypeName,
                    DisplayName = definition.DisplayNameText
                })
                .OrderBy(option => option.DisplayLabel, System.StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            options.Insert(0, new BlockDefinitionOption { TypeId = string.Empty, SubtypeId = string.Empty, DisplayName = ModText.BC_UI_Desc_AnyBlock.GetString() });

            return options;
        }

        private static List<SkinOption> BuildSkins() {
            var steamUserId = MyAPIGateway.Session?.LocalHumanPlayer?.SteamUserId;
            _skinsFilteredByOwnership = steamUserId.HasValue;

            var options = MyDefinitionManager.Static
                .GetAssetModifierDefinitions()
                .Where(definition => definition.Public
                    && !string.IsNullOrEmpty(definition.Id.SubtypeName)
                    && definition.Icons != null
                    && definition.Icons.Length > 0
                    && IsOwned(definition, steamUserId))
                .Select(definition => new SkinOption {
                    SkinId = definition.Id.SubtypeName,
                    DisplayName = string.IsNullOrWhiteSpace(definition.DisplayNameText) ? definition.Id.SubtypeName : definition.DisplayNameText,
                    IconMaterial = ResolveIconMaterial(definition.Id.SubtypeName)
                })
                .OrderBy(option => option.DisplayName, System.StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            options.Insert(0, new SkinOption { SkinId = string.Empty, DisplayName = ModText.BC_UI_NoSkinDefault.GetString() });

            return options;
        }

        /// <summary>
        /// True when the skin needs no DLC or the player owns the DLC it needs.
        /// </summary>
        private static bool IsOwned(MyAssetModifierDefinition definition, ulong? steamUserId) {
            if (definition.DLCs == null || definition.DLCs.Length == 0) {
                return true;
            }

            if (!steamUserId.HasValue) {
                return true;
            }

            return MyAPIGateway.DLC.HasDefinitionDLC(definition.Id, steamUserId.Value);
        }

        /// <summary>
        /// Returns the icon material declared for a skin, or null when this mod ships none for it.
        /// </summary>
        private static string ResolveIconMaterial(string skinId) {
            var subtype = SkinOption.ICON_MATERIAL_PREFIX + skinId;
            var definitionId = new MyDefinitionId(typeof(MyObjectBuilder_TransparentMaterialDefinition), subtype);

            MyTransparentMaterialDefinition definition;
            return MyDefinitionManager.Static.TryGetDefinition(definitionId, out definition) ? subtype : null;
        }

        public class BlockDefinitionOption {
            public string TypeId { get; set; }
            public string SubtypeId { get; set; }
            public string DisplayName { get; set; }

            public string DisplayLabel {
                get {
                    if (string.IsNullOrEmpty(TypeId) && string.IsNullOrEmpty(SubtypeId)) {
                        return DisplayName;
                    }

                    return string.IsNullOrWhiteSpace(DisplayName)
                        ? SubtypeId
                        : string.Format("{0} ({1})", DisplayName, SubtypeId);
                }
            }
        }

        public class SkinOption {
            /// <summary>
            /// Prefix of the transparent materials holding the vanilla armor skin icons.
            /// </summary>
            public const string ICON_MATERIAL_PREFIX = "BuildColors_Skin_";

            public string SkinId { get; set; }
            public string DisplayName { get; set; }

            /// <summary>
            /// Subtype of the transparent material holding this skin's icon, or null when none is declared.
            /// </summary>
            public string IconMaterial { get; set; }
        }
    }
}
