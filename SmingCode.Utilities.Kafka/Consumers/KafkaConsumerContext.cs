namespace SmingCode.Utilities.Kafka.Consumers;

public class KafkaConsumerContext
{
    internal KafkaConsumerContext(
        IKafkaConsumer parentKafkaConsumer,
        string topicConsumed,
        int partitionNo,
        HeaderCollection headers,
        object? key,
        Type keyType,
        object? value,
        Type valueType,
        ICustomPropertyHandler customPropertyHandler,
        Func<KafkaConsumerContext, Task<KafkaEventResult>> messageConsumer,
        IServiceProvider serviceProvider
    ) => (ParentKafkaConsumer, TopicConsumed, PartitionNo, Headers, Key, KeyType, Value, ValueType, CustomPropertyHandler, MessageConsumer, ServiceProvider)
            = (parentKafkaConsumer, topicConsumed, partitionNo, headers, key, keyType, value, valueType, customPropertyHandler, messageConsumer, serviceProvider);

    internal IKafkaConsumer ParentKafkaConsumer { get; }
    internal IServiceProvider ServiceProvider { get; }
    internal Func<KafkaConsumerContext, Task<KafkaEventResult>> MessageConsumer { get; }

    public string TopicConsumed { get; }
    public int PartitionNo { get; }
    public HeaderCollection Headers { get; }
    public object? Key { get; }
    public Type KeyType { get; }
    public object? Value { get; }
    public Type ValueType { get; }
    public async Task PauseTopicPartition(
        TimeSpan pause
    ) => await ParentKafkaConsumer.PauseTopicPartition(
        TopicConsumed,
        PartitionNo,
        pause
    );
    public ICustomPropertyHandler CustomPropertyHandler { get; }
}
