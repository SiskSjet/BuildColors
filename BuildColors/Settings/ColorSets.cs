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
        /// </summary>
        public const int VERSION = 2;

        public ColorSets() : base(new ColorSetComparer()) { }

        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public int Version { get; set; } = VERSION;

        /// <summary>
        /// Fills in the masks of any set written before version 2.
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
