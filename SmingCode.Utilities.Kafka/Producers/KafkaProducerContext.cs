using System.Text;

namespace SmingCode.Utilities.Kafka.Producers;

public class KafkaProducerContext
{
    private readonly object? _key;

    internal KafkaProducerContext(
        string topic,
        object? key,
        Type keyType,
        object value,
        Type valueType,
        Headers headers,
        Func<KafkaProducerContext, Task<bool>> messageProducer,
        IServiceProvider serviceProvider
    ) => (Topic, _key, KeyType, Value, ValueType, Headers, MessageProducer, ServiceProvider)
            = (topic, key, keyType, value, valueType, headers, messageProducer, serviceProvider);

    internal Func<KafkaProducerContext, Task<bool>> MessageProducer { get; }
    internal IServiceProvider ServiceProvider { get; }
    public string Topic { get; }
    public object Key => _key
        ?? throw new InvalidOperationException("Attempt to retrieve the Key value when it has not been set. Please check HasKey first.");
    public Type KeyType { get; }
    public object Value { get; }
    public Type ValueType { get; }
    public Headers Headers { get; }

    public KafkaProducerContext AddHeader(
        string headerName,
        string value
    )
    {
        Headers.Add(new Header(
            headerName,
            Encoding.UTF8.GetBytes(value)
        ));

        return this;
    }

    public bool HasKey => _key is not null;
}
