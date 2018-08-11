using System.Collections.Generic;
using ProtoBuf;
using Sisk.BuildColors.Settings.Models;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings {
    [ProtoContract]
    public class ModSettings {
        public const int VERSION = 1;

        [ProtoMember(2)]
        public HashSet<PlayerColors> Colors { get; set; } = new HashSet<PlayerColors>(new PlayerColorComparer());

        [ProtoMember(1)]
        public int Version { get; set; } = VERSION;
    }
}