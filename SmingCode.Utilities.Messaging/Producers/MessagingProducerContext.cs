namespace SmingCode.Utilities.Messaging.Producers;

public class MessagingProducerContext
{
    private readonly object? _value;

    internal MessagingProducerContext(
        string topic,
        object? value,
        Type valueType,
        MetadataCollection metadataCollection,
        Func<MessagingProducerContext, Task<bool>> messageProducer,
        IServiceProvider serviceProvider
    ) => (Topic, _value, ValueType, MetadataCollection, MessageProducer, ServiceProvider)
            = (topic, value, valueType, metadataCollection, messageProducer, serviceProvider);

    internal Func<MessagingProducerContext, Task<bool>> MessageProducer { get; }
    internal IServiceProvider ServiceProvider { get; }
    public string Topic { get; }
    public object Value => _value
        ?? throw new InvalidOperationException("Attempt to retrieve the message value when it has not been set. Please check HasValue first.");
    public Type ValueType { get; }
    public MetadataCollection MetadataCollection { get; }

    public bool HasValue => _value is not null;
}
