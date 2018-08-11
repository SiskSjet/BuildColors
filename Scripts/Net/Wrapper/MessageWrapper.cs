using System;
using ProtoBuf;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Net.Wrapper {
    [ProtoContract]
    internal sealed class MessageWrapper : Wrapper {
        /// <summary>
        ///     A default private constructor for protobuf-net.
        /// </summary>
        private MessageWrapper() { }

        public MessageWrapper(string type) {
            if (string.IsNullOrWhiteSpace(type)) {
                throw new ArgumentNullException(nameof(type), $"'{nameof(type)}' can not be null or whitespace.");
            }

            Type = type;
        }

        [ProtoMember(1)]
        public string Type { get; set; }
    }
}