namespace SmingCode.Utilities.Messaging.Kafka.Consumers;

public interface IKafkaConsumerContext
{
    int PartitionNo { get; }
    Task PauseTopicPartition(
        TimeSpan delay
    );
}