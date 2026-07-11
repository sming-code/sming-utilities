using System.Text;

namespace SmingCode.Utilities.Kafka.Producers;

internal class DirectKeyPartitionCorrelationTopicPartitioner(
    string topic
) : ITopicPartitioner
{
    private readonly PartitionerDelegate? _partitionerDelegate = (topic, _, key, _)
        => new Partition(
            int.TryParse(
                Encoding.UTF8.GetString(key),
                out var keyValue
            ) ? keyValue : 0);

    public ProducerBuilder<TKey, TValue> GetPartitionedProducerBuilder<TKey, TValue>(
        ProducerBuilder<TKey, TValue> producerBuilder
    ) => producerBuilder
            .SetPartitioner(
                topic,
                _partitionerDelegate
            );
}