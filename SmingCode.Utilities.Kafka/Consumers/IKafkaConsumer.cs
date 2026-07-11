namespace SmingCode.Utilities.Kafka.Consumers;

internal interface IKafkaConsumer
{
    void InitialiseEventConsumer(
        CancellationToken cancellationToken
    );
    Task PauseTopicPartition(
        string topicName,
        int partitionNo,
        TimeSpan delay
    );
}
