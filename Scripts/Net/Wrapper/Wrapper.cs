using System;
using ProtoBuf;
using Sandbox.ModAPI;
using Sisk.BuildColors.Net.Messages;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Net.Wrapper {
    [ProtoContract]
    [ProtoInclude(10, typeof(MessageWrapper))]
    [ProtoInclude(11, typeof(EntityMessageWrapper))]
    internal abstract class Wrapper {
        [ProtoMember(2)]
        public byte[] Content { get; set; }

        [ProtoMember(1)]
        public ulong Sender { get; set; }

        public static TType GetContent<TType>(Wrapper wrapper) {
            return wrapper.GetContent<TType>();
        }

        public TType GetContent<TType>() {
            TType content;
            try {
                content = MyAPIGateway.Utilities.SerializeFromBinary<TType>(Content);
            } catch (Exception exception) {
                throw new Exception($"Can't deserialize content as '{typeof(TType)}'.", exception);
            }

            return content;
        }
    }
}