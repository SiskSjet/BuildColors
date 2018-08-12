using Sisk.Utils.Localization;
using VRage.Utils;

// ReSharper disable InconsistentNaming
namespace Sisk.BuildColors.Localization {
    public static class BuildColorsText {
        public static readonly MyStringId ColorSetRemoved = Localize.Get(nameof(ColorSetRemoved));
        public static readonly MyStringId ColorSetSaved = Localize.Get(nameof(ColorSetSaved));
        public static readonly MyStringId Description_Help = Localize.Get(nameof(Description_Help));
        public static readonly MyStringId Description_List = Localize.Get(nameof(Description_List));
        public static readonly MyStringId Description_Load = Localize.Get(nameof(Description_Load));
        public static readonly MyStringId Description_Remove = Localize.Get(nameof(Description_Remove));
        public static readonly MyStringId Description_Save = Localize.Get(nameof(Description_Save));

        public static readonly MyStringId NoColorSetFound = Localize.Get(nameof(NoColorSetFound));
        public static readonly MyStringId NoColorSetsAvailable = Localize.Get(nameof(NoColorSetsAvailable));

        /// <summary>
        ///     Creates all translation for this mod.
        /// </summary>
        public static void CreateTranslations() {
            Localize.Create(nameof(Description_Save), "Saves a Color Set with the given name.", German: "Speichert ein Color Set mit dem angegebenen namen.");
            Localize.Create(nameof(Description_Load), "Loads a Color Set with the given name.", German: "Lädt ein Color Set mit dem angegebenen Namen.");
            Localize.Create(nameof(Description_Remove), "Removes a Color Set with given name.", German: "Entfernt ein Color Set mit dem angegebenen Namen.");
            Localize.Create(nameof(Description_List), "Lists all available color sets.", German: "Listet alle verfügbaren Color Sets auf.");
            Localize.Create(nameof(Description_Help), "Shows this help page.", German: "Zeigt diese Hilfe-Seite");

            Localize.Create(nameof(NoColorSetFound), "No color set with name '{0}' found.", German: "Kein Color Set mit dem namen {0} gefunden.");
            Localize.Create(nameof(NoColorSetsAvailable), "No color sets available.", German: "Keine Color Sets verfügbar.");
            Localize.Create(nameof(ColorSetRemoved), "Color set '{0}' removed.", German: "Color Set '{0}' entfernt.");
            Localize.Create(nameof(ColorSetSaved), "Color set '{0}' saved.", German: "Color Set '{0}' gespeichert.");
        }
    }
}