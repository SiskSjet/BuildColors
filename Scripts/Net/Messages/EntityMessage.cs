using ProtoBuf;
using Sandbox.ModAPI;
using Sisk.BuildColors.Net.Wrapper;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Net.Messages {
    [ProtoContract]
    internal class EntityMessage : IMessage {
        [ProtoMember(1)]
        public EntityMessageWrapper Wrapper { get; set; }

        public byte[] Serialze() {
            return MyAPIGateway.Utilities.SerializeToBinary(this);
        }
    }
}