using System.Collections.Generic;
using ProtoBuf;
using Sandbox.ModAPI;
using Sisk.Utils.Net.Messages;
using VRageMath;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Net.Messages {
    [ProtoContract]
    public class BuildColorMessage : IMessage {
        [ProtoMember(2)]
        public List<Vector3> BuildColors { get; set; }

        [ProtoMember(1)]
        public ulong SteamId { get; set; }

        public byte[] Serialize() {
            return MyAPIGateway.Utilities.SerializeToBinary(this);
        }
    }
}
