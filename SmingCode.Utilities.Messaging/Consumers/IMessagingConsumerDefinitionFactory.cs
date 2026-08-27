namespace SmingCode.Utilities.Messaging.Consumers;

internal interface IMessagingConsumerDefinitionFactory
{
    IMessagingConsumerDefinition GetMessageConsumerDefinition(
        string topicToMatch,
        Delegate handler,
        IServiceCollection services
    );
}
