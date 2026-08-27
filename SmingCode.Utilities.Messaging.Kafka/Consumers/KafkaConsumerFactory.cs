namespace SmingCode.Utilities.Messaging.Kafka.Consumers;

internal class KafkaConsumerFactory(
    IServiceProvider _serviceProvider
) : IMessageConsumerFactory
{
    public IMessageConsumer GetMessageConsumer(
        IMessagingConsumerDefinition consumerDefinition
    )
    {
        var consumerType = typeof(KafkaMessageConsumer<>)
            .MakeGenericType(consumerDefinition.GetType().GetGenericArguments());

        return (IMessageConsumer)ActivatorUtilities.CreateInstance(
            _serviceProvider,
            consumerType,
            [ consumerDefinition ]
        );
    }
}