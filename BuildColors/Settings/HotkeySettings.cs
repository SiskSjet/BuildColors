using RichHudFramework.UI;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings {

    [XmlRoot(nameof(HotkeySettings))]
    public class HotkeySettings {
        public const int VERSION = 1;

        [XmlAttribute]
        public int Version { get; set; } = VERSION;

        [XmlArray("Groups")]
        [XmlArrayItem("Group")]
        public List<HotkeyGroup> Groups { get; set; } = new List<HotkeyGroup>();
    }

    public class HotkeyGroup {

        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// An empty control list is a bind the player removed, and is kept as such.
        /// </summary>
        [XmlArray("Binds")]
        public BindDefinition[] Binds { get; set; }
    }
}
