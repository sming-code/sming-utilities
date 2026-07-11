namespace SmingCode.Utilities.Kafka.Producers;

internal interface IKafkaProducerBuilder
{
    IProducer<string, string> Producer { get; }
}
