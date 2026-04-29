namespace SmingCode.Utilities.Kafka.Producers;

public interface IKafkaProducer
{
    Task<bool> SendEvent<TValue>(
        string topic,
        TValue value
    ) where TValue : notnull;
    Task<bool> SendEvent<TKey, TValue>(
        string topic,
        TKey key,
        TValue value
    ) where TValue : notnull;
    Task<bool> SendEvent<TValue>(
        string topic,
        TValue value,
        Headers headers
    ) where TValue : notnull;
    Task<bool> SendEvent<TKey, TValue>(
        string topic,
        TKey key,
        TValue value,
        Headers headers
    ) where TValue : notnull;
}