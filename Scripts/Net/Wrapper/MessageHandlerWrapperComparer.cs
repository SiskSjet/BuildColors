using System.Collections.Generic;

namespace Sisk.BuildColors.Net.Wrapper {
    internal class MessageHandlerWrapperComparer : IEqualityComparer<MessageHandlerWrapper> {
        public bool Equals(MessageHandlerWrapper wrapper, MessageHandlerWrapper wrapper2) {
            return wrapper != null && wrapper2 != null && wrapper.HashCode.Equals(wrapper2.HashCode);
        }

        public int GetHashCode(MessageHandlerWrapper item) {
            return item.HashCode;
        }
    }
}