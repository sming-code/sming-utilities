namespace SmingCode.Utilities.Messaging.Producers;

public interface IMessagingProducer
{
    Task<bool> SendMessage(
        string topic
    );
    Task<bool> SendMessage(
        string topic,
        MetadataCollection metadataCollection
    );
    Task<bool> SendMessage<TValue>(
        string topic,
        TValue value
    ) where TValue : notnull;
    Task<bool> SendMessage<TValue>(
        string topic,
        TValue value,
        MetadataCollection metadataCollection
    ) where TValue : notnull;
}