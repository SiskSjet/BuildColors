using Sisk.BuildColors.Net.Messages;

namespace Sisk.BuildColors.Net.Delegates {
    public delegate void EntityMessageHandler<in TMessageType>(ulong sender, TMessageType message) where TMessageType : IEntityMessage;
}