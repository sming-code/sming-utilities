namespace SmingCode.Utilities.Messaging.Kafka.Config;

public interface IKafkaHandlingConfigurationBuilder
{
    IKafkaHandlingConfigurationBuilder AddKafkaConsumers();
    IKafkaHandlingConfigurationBuilder UseKafkaProducer();
}
