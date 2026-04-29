namespace SmingCode.Utilities.Kafka.Consumers;

public class KafkaConsumerContext
{
    internal KafkaConsumerContext(
        string topicConsumed,
        object consumeResult,
        Headers headers,
        Type keyType,
        Type valueType,
        Func<KafkaConsumerContext, Task<KafkaEventResult>> messageConsumer,
        IServiceProvider serviceProvider
    ) => (TopicConsumed, ConsumeResult, Headers, KeyType, ValueType, MessageConsumer, ServiceProvider)
            = (topicConsumed, consumeResult, headers, keyType, valueType, messageConsumer, serviceProvider);

    internal IServiceProvider ServiceProvider { get; }
    internal Func<KafkaConsumerContext, Task<KafkaEventResult>> MessageConsumer { get; }

    public string TopicConsumed { get; }
    public object ConsumeResult { get; }
    public Headers Headers { get; }
    public Type KeyType { get; }
    public Type ValueType { get; }
}
