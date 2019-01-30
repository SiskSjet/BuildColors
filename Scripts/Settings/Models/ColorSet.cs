using System;
using System.Xml.Serialization;
using ProtoBuf;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings.Models {
    [ProtoContract]
    public struct ColorSet : IEquatable<ColorSet>, IEquatable<string> {
        public ColorSet(string name, Color[] colors) {
            Name = name;
            Colors = colors;
        }

        [ProtoMember(2)]
        [XmlArray(Order = 2)]
        [XmlArrayItem]
        public Color[] Colors { get; set; }

        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public string Name { get; set; }

        public bool Equals(ColorSet other) {
            return StringComparer.InvariantCultureIgnoreCase.Equals(Name, other.Name);
        }

        public bool Equals(string other) {
            return StringComparer.InvariantCultureIgnoreCase.Equals(Name, other);
        }
    }
}
