using ProtoBuf;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings.Models {
    [ProtoContract]
    public struct PlayerColors {
        [ProtoMember(2)]
        public byte[] Colors { get; set; }

        [ProtoMember(1)]
        public ulong Id { get; set; }
    }
}