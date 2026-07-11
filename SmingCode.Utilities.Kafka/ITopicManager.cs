namespace SmingCode.Utilities.Kafka;

internal interface ITopicManager
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
