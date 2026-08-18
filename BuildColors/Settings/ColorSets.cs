using ProtoBuf;
using Sisk.BuildColors.Settings.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings {

    [ProtoContract]
    [XmlRoot(nameof(ColorSets))]
    public class ColorSets : HashSet<ColorSet> {
        /// <summary>
        /// Version 2 keeps the slots as the game holds them instead of as eight bit RGB.
        /// Version 3 saves them as SE HSV, the game's own picker values, instead of the game's offset mask.
        /// </summary>
        public const int VERSION = 3;

        public ColorSets() : base(new ColorSetComparer()) { }

        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public int Version { get; set; } = VERSION;

        /// <summary>
        /// Fills in the masks of any set written before the current version, and re-saves it in that version.
        /// </summary>
        public void Upgrade() {
            var upgraded = this.Select(set => set.Upgraded()).ToList();

            Clear();

            foreach (var set in upgraded) {
                Add(set);
            }

            Version = VERSION;
        }
    }
}
