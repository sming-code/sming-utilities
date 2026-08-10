namespace SmingCode.Utilities.Messaging.Kafka.Producers;

internal interface IKafkaProducerBuilder
{
    IProducer<string, string> Producer { get; }
}
