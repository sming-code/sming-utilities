namespace SmingCode.Utilities.Messaging.Consumers;

internal interface IMessageConsumerFactory
{
    IMessageConsumer GetMessageConsumer(
        IMessagingConsumerDefinition consumerDefinition
    );
}
