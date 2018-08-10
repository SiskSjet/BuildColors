using ProtoBuf;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings {
    [ProtoContract]
    public class PlayerColors {
        [ProtoMember(2)]
        public byte[] Colors { get; set; }

        [ProtoMember(1)]
        public ulong Id { get; set; }
    }
}