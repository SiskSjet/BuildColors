using ProtoBuf;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings {
    [ProtoContract]
    public class ModSettings {
        public const int VERSION = 1;

        [ProtoMember(2)]
        public PlayerColors[] Colors { get; set; }

        [ProtoMember(1)]
        public int Version { get; set; } = VERSION;
    }
}