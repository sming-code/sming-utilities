namespace SmingCode.Utilities.Messaging.Kafka.Producers;

public interface IMessagingProducerWithKeyValues
{
    Task<bool> SendMessage<TKey, TBody>(
        string topic,
        TKey key,
        TBody value
    ) where TKey : notnull
      where TBody : notnull;
    Task<bool> SendMessage<TKey, TBody>(
        string topic,
        TKey key,
        TBody value,
        MetadataCollection? metadataCollection = null
    ) where TKey : notnull
      where TBody : notnull;
}