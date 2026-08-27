namespace SmingCode.Utilities.ProcessTracking.Messaging.Config;
using Utilities.Messaging.Consumers;
using Utilities.Messaging.Config;

public static class Injection
{
    public static IValidProcessTrackingBuilder AddKafkaMiddleware(
        this IProcessTrackingBuilder builder
    )
    {
        var services = ((IProcessTrackingBuilderInternal)builder).Services;
        services.AddMessagingConsumerMiddleware<ProcessTrackingConsumerMiddleware>();
        services.AddMessagingProducerMiddleware<ProcessTrackingProducerMiddleware>();

        return new ValidProcessTrackingBuilder(services);
    }

    public static IMessagingConsumerDefinition InitialisesProcess(
        this IMessagingConsumerDefinition kafkaConsumerDefinition,
        string processName
    )
    {
        kafkaConsumerDefinition.CustomPropertyHandler.TryAddCustomProperty(
            Constants.INITIALISES_PROCESS_CUSTOM_PROPERTY_NAME, true
        );
        kafkaConsumerDefinition.CustomPropertyHandler.TryAddCustomProperty(
            Constants.PROCESS_NAME_CUSTOM_PROPERTY_NAME, processName
        );

        return kafkaConsumerDefinition;
    }
}