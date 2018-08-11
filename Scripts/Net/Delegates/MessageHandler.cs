using Sisk.BuildColors.Net.Messages;

namespace Sisk.BuildColors.Net.Delegates {
    public delegate void MessageHandler<in TMessageType>(ulong sender, TMessageType message) where TMessageType : IMessage;
}