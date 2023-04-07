using ProtoBuf;
using Sisk.BuildColors.Settings.Models;
using System.Collections.Generic;
using System.Xml.Serialization;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings {

    [ProtoContract]
    public class ModSettings {
        public const int VERSION = 1;

        [ProtoMember(2)]
        [XmlArray(Order = 2)]
        public HashSet<PlayerColors> Colors { get; set; } = new HashSet<PlayerColors>(new PlayerColorComparer());

        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public int Version { get; set; } = VERSION;
    }
}