namespace SmingCode.Utilities.Messaging.Kafka;

internal interface ITopicPartitioner
{
    ProducerBuilder<TKey, TValue> GetPartitionedProducerBuilder<TKey, TValue>(
        ProducerBuilder<TKey, TValue> producerBuilder
    );
}
