using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using VRageMath;

namespace Sisk.BuildColors.Settings.Models {

    /// <summary>
    /// The build colors a server was last left with, so they can be put back on returning to it.
    /// </summary>
    [ProtoContract]
    public struct ServerEntry {
        [ProtoMember(1)]
        [XmlAttribute()]
        public string Id { get; set; }

        /// <summary>
        /// Colors as written before version 2. Read so old files still restore, never written again.
        /// </summary>
        [ProtoMember(2)]
        [XmlArray(Order = 2)]
        [XmlArrayItem]
        public Color[] Colors { get; set; }

        /// <summary>
        /// Slots as the game's offset mask, written before SE HSV. Read only, never written again.
        /// </summary>
        [ProtoMember(3)]
        [XmlArray(ElementName = "Masks", Order = 3)]
        [XmlArrayItem]
        public ColorMask[] LegacyMasks { get; set; }

        [ProtoMember(4)]
        [XmlArray(Order = 4)]
        [XmlArrayItem]
        public SeHsv[] Hsv { get; set; }

        public static ServerEntry FromSlots(string id, IEnumerable<Vector3> slots) {
            var hsv = new List<SeHsv>(ColorSet.SLOTS);

            foreach (var slot in slots) {
                hsv.Add((ColorMask)slot);
            }

            return new ServerEntry { Id = id, Hsv = hsv.ToArray() };
        }

        public List<Vector3> ToBuildColorSlots() {
            return new ColorSet { Colors = Colors, LegacyMasks = LegacyMasks, Hsv = Hsv }.ToBuildColorSlots();
        }
    }
}
