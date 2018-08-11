using System;
using Sisk.BuildColors.Net.Delegates;
using Sisk.BuildColors.Net.Messages;

namespace Sisk.BuildColors.Net.Wrapper {
    internal class MessageHandlerWrapper {
        private Action<ulong, object> Action { get; set; }
        private Func<Net.Wrapper.Wrapper, object> DeserializeAction { get; set; }
        public int HashCode { get; private set; }

        public static MessageHandlerWrapper Create<TMessageType>(MessageHandler<TMessageType> handler) where TMessageType : IMessage {
            return new MessageHandlerWrapper { Action = (sender, message) => handler(sender, (TMessageType) message), HashCode = handler.Method.GetHashCode(), DeserializeAction = wrapper => Net.Wrapper.Wrapper.GetContent<TMessageType>(wrapper) };
        }

        public static MessageHandlerWrapper Create<TMessageType>(EntityMessageHandler<TMessageType> handler) where TMessageType : IEntityMessage {
            return new MessageHandlerWrapper { Action = (sender, message) => handler(sender, (TMessageType) message), HashCode = handler.Method.GetHashCode(), DeserializeAction = wrapper => Net.Wrapper.Wrapper.GetContent<TMessageType>(wrapper) };
        }

        public object Deserialize(MessageWrapper wrapper) {
            return DeserializeAction(wrapper);
        }

        public object Deserialize(EntityMessageWrapper wrapper) {
            return DeserializeAction(wrapper);
        }

        public void Invoke(ulong sender, object message) {
            Action(sender, message);
        }
    }
}