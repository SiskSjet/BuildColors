using ProtoBuf;
using Sisk.BuildColors.Settings.Models;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings {

    [ProtoContract]
    [XmlRoot(nameof(ServerMemory))]
    public class ServerMemory {
        public const int VERSION = 1;

        [ProtoMember(2)]
        [XmlArray(Order = 2)]
        [XmlArrayItem]
        public HashSet<ServerEntry> ServerEntries { get; set; } = new HashSet<ServerEntry>();

        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public int Version { get; set; } = VERSION;
    }
}