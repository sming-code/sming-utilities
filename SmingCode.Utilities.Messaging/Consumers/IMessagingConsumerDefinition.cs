namespace SmingCode.Utilities.Messaging.Consumers;

public interface IMessagingConsumerDefinition
{
    Guid ConsumerId { get; }
    string TopicToMatch { get; }
    IMessagingConsumerDefinition WithIsolationMode(
        IsolationMode isolationMode
    );
    IMessagingConsumerDefinition UseRegexPatternMatchingForTopic();
    ICustomPropertyHandler CustomPropertyHandler { get; }
}

internal interface IMessagingConsumerDefinitionInternal
{
    IServiceCollection Services { get; }
}