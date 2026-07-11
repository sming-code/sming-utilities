namespace SmingCode.Utilities.Kafka.Producers;

public class KafkaProducerContext
{
    private readonly object? _key;
    private readonly object? _value;

    internal KafkaProducerContext(
        string topic,
        object? key,
        Type keyType,
        object? value,
        Type valueType,
        HeaderCollection headers,
        Func<KafkaProducerContext, Task<bool>> messageProducer,
        IServiceProvider serviceProvider
    ) => (Topic, _key, KeyType, _value, ValueType, Headers, MessageProducer, ServiceProvider)
            = (topic, key, keyType, value, valueType, headers, messageProducer, serviceProvider);

    internal Func<KafkaProducerContext, Task<bool>> MessageProducer { get; }
    internal IServiceProvider ServiceProvider { get; }
    public string Topic { get; }
    public object Key => _key
        ?? throw new InvalidOperationException("Attempt to retrieve the Key value when it has not been set. Please check HasKey first.");
    public Type KeyType { get; }
    public object Value => _value
        ?? throw new InvalidOperationException("Attempt to retrieve the message value when it has not been set. Please check HasValue first.");
    public Type ValueType { get; }
    public HeaderCollection Headers { get; }

    public bool HasKey => _key is not null;
    public bool HasValue => _value is not null;
}
