namespace SmingCode.Utilities.Kafka.Consumers;

public interface IKafkaConsumerDefinition
{
    string TopicToMatch { get; }
    IKafkaConsumerDefinition WithIsolationMode(
        IsolationMode isolationMode
    );
    IKafkaConsumerDefinition UseRegexPatternMatchingForTopic();
    IKafkaConsumerDefinition CreateTopicIfNotExists();
    ICustomPropertyHandler CustomPropertyHandler { get; }
}

internal interface IKafkaConsumerDefinitionInternal
{
    IServiceCollection Services { get; }
    IKafkaConsumerDefinition WithPreInitProcessHandler<IHandler>()
        where IHandler : IKafkaConsumerPreInitProcessHandler;
}