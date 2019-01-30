using System.Collections.Generic;
using System.Xml.Serialization;
using ProtoBuf;
using Sisk.BuildColors.Settings.Models;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings {
    [ProtoContract]
    [XmlRoot(nameof(ColorSets))]
    public class ColorSets : HashSet<ColorSet> {
        public const int VERSION = 1;

        public ColorSets() : base(new ColorSetComparer()) { }

        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public int Version { get; set; } = VERSION;
    }
}
