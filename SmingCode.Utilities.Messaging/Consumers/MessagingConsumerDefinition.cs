namespace SmingCode.Utilities.Messaging.Consumers;

internal class MessagingConsumerDefinition<TBody>(
    string topicToMatch,
    Delegate handler,
    IServiceCollection services
) : IMessagingConsumerDefinition, IMessagingConsumerDefinitionInternal
{
    private readonly Guid _consumerId = Guid.NewGuid();
    private readonly CustomPropertyHandler _customPropertyHandler = new();
    internal MessageConsumerDelegateInvoker<TBody> Handler { get; } = new(handler);

    public IServiceCollection Services { get; } = services;
    public Guid ConsumerId => _consumerId;
    internal IsolationMode IsolationMode { get; private set; } = IsolationMode.PerServiceType;
    internal bool UseRegexPatternMatching { get; private set; }
    internal bool CreateTopic { get; private set; }

    public string TopicToMatch { get; } = topicToMatch;

    public IMessagingConsumerDefinition WithIsolationMode(
        IsolationMode isolationMode
    )
    {
        IsolationMode = isolationMode;

        return this;
    }

    public IMessagingConsumerDefinition UseRegexPatternMatchingForTopic()
    {
        UseRegexPatternMatching = true;

        return this;
    }

    public IMessagingConsumerDefinition CreateTopicIfNotExists()
    {
        CreateTopic = true;

        return this;
    }

    public ICustomPropertyHandler CustomPropertyHandler => _customPropertyHandler;
}
