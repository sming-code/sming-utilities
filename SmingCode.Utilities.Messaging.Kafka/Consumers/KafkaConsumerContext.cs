namespace SmingCode.Utilities.Messaging.Kafka.Consumers;

public class KafkaConsumerContext : MessagingConsumerContext, IKafkaConsumerContext
{
    internal KafkaConsumerContext(
        IMessageConsumer parentMessageConsumer,
        string topicConsumed,
        int partitionNo,
        MetadataCollection metadataCollection,
        object? value,
        Type valueType,
        ICustomPropertyHandler customPropertyHandler,
        Func<MessagingConsumerContext, Task> messageConsumer,
        IServiceProvider serviceProvider
    ) : base(
        parentMessageConsumer,
        topicConsumed,
        metadataCollection,
        value,
        valueType,
        customPropertyHandler,
        messageConsumer,
        serviceProvider
    ) => PartitionNo = partitionNo;

    public int PartitionNo { get; }
    public async Task PauseTopicPartition(
        TimeSpan delay
    ) => await ((IKafkaMessageConsumer)Consumer).PauseTopicPartition(
        TopicConsumed,
        PartitionNo,
        delay
    );
}
