namespace SmingCode.Utilities.Messaging.Consumers;

public class MessagingConsumerContext
{
    internal MessagingConsumerContext(
        IMessageConsumer consumer,
        string topicConsumed,
        MetadataCollection metadataCollection,
        object? value,
        Type valueType,
        ICustomPropertyHandler customPropertyHandler,
        Func<MessagingConsumerContext, Task> messageConsumer,
        IServiceProvider serviceProvider
    ) => (Consumer, TopicConsumed, MetadataCollection, Value, ValueType, CustomPropertyHandler, MessageConsumer, ServiceProvider)
            = (consumer, topicConsumed, metadataCollection, value, valueType, customPropertyHandler, messageConsumer, serviceProvider);

    internal IMessageConsumer Consumer { get; }
    internal IServiceProvider ServiceProvider { get; }
    internal Func<MessagingConsumerContext, Task> MessageConsumer { get; }

    public string TopicConsumed { get; }
    public MetadataCollection MetadataCollection { get; }
    public object? Value { get; }
    public Type ValueType { get; }
    public ICustomPropertyHandler CustomPropertyHandler { get; }
}
