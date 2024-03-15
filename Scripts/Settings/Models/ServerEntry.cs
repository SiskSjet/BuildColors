using ProtoBuf;
using System.Xml.Serialization;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings.Models {

    [ProtoContract]
    public struct ServerEntry {

        [ProtoMember(2)]
        [XmlArray(Order = 2)]
        [XmlArrayItem]
        public Color[] Colors { get; set; }

        [ProtoMember(1)]
        [XmlAttribute()]
        public ulong Id { get; set; }
    }
}