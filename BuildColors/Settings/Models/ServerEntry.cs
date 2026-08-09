using ProtoBuf;
using System.Collections.Generic;
using System.Xml.Serialization;
using VRageMath;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings.Models {

    /// <summary>
    /// The palette a player last built with on one server.
    /// </summary>
    [ProtoContract]
    public struct ServerEntry {
        /// <summary>
        /// Colors as written before version 2, kept so an old file still restores a palette.
        /// </summary>
        [ProtoMember(2)]
        [XmlArray(Order = 2)]
        [XmlArrayItem]
        public Color[] Colors { get; set; }

        [ProtoMember(1)]
        [XmlAttribute()]
        public string Id { get; set; }

        /// <summary>
        /// The slots as the game holds them.
        /// </summary>
        [ProtoMember(3)]
        [XmlArray(Order = 3)]
        [XmlArrayItem]
        public ColorMask[] Masks { get; set; }

        public static ServerEntry FromSlots(string id, IEnumerable<Vector3> slots) {
            var masks = new List<ColorMask>(ColorSet.SLOTS);

            foreach (var slot in slots) {
                masks.Add(slot);
            }

            return new ServerEntry { Id = id, Masks = masks.ToArray() };
        }

        public List<Vector3> ToBuildColorSlots() {
            return new ColorSet { Colors = Colors, Masks = Masks }.ToBuildColorSlots();
        }
    }
}
