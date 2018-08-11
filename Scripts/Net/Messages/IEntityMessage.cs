namespace Sisk.BuildColors.Net.Messages {
    public interface IEntityMessage {
        long EntityId { get; set; }
        byte[] Serialze();
    }
}