namespace SmingCode.Utilities.Kafka.Producers;

public interface IKafkaProducer
{
    Task<bool> SendEvent(
        string topic
    );
    Task<bool> SendEvent(
        string topic,
        HeaderCollection headers
    );
    Task<bool> SendEvent<TValue>(
        string topic,
        TValue value
    ) where TValue : notnull;
    Task<bool> SendEvent<TKey, TValue>(
        string topic,
        TKey key,
        TValue value
    ) where TKey : notnull where TValue : notnull;
    Task<bool> SendEvent<TValue>(
        string topic,
        TValue value,
        HeaderCollection headers
    ) where TValue : notnull;
    Task<bool> SendEvent<TKey, TValue>(
        string topic,
        TKey key,
        TValue value,
        HeaderCollection headers
    ) where TKey : notnull where TValue : notnull;
}
