namespace SmingCode.Utilities.Messaging.Kafka.Consumers;

internal interface IKafkaConsumerPreInitProcessHandler
{
    Task Run(IMessagingConsumerDefinition consumerDefinition);
}
