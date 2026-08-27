namespace SmingCode.Utilities.Messaging.Kafka;

internal interface IKafkaAdminClient
{
    Task<bool> CreateTopic(
        string topicName,
        int noPartitions = 1,
        short replicationFactor = 1
    );
    Task<bool> RemoveTopic(
        string topicName
    );
}
