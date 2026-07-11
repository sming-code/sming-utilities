namespace SmingCode.Utilities.Kafka.Producers;

internal interface ITopicPartitioner
{
    ProducerBuilder<TKey, TValue> GetPartitionedProducerBuilder<TKey, TValue>(
        ProducerBuilder<TKey, TValue> producerBuilder
    );
}
