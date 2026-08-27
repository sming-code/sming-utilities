namespace SmingCode.Utilities.Messaging.Kafka;

internal class TopicPartitionerFactory
{
    private readonly List<ITopicPartitioner> _topicPartitioners = [];

    internal void AddTopicPartitioner(
        ITopicPartitioner topicPartitioner
    ) => _topicPartitioners.Add(topicPartitioner);

    internal List<ITopicPartitioner> GetTopicPartitioners()
        => _topicPartitioners;
}