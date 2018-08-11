using ProtoBuf;
using Sandbox.ModAPI;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Net.Messages {
    [ProtoContract]
    public class RequestBuildColors : IMessage {
        [ProtoMember(1)]
        public ulong SteamId { get; set; }

        public byte[] Serialze() {
            return MyAPIGateway.Utilities.SerializeToBinary(this);
        }
    }
}