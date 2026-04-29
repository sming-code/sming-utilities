namespace SmingCode.Utilities.ProcessTracking.Kafka.Config;
using Utilities.Kafka.Config;

public static class Injection
{
    public static IValidProcessTrackingBuilder AddKafkaMiddleware(
        this IProcessTrackingBuilder builder
    )
    {
        var services = ((IProcessTrackingBuilderInternal)builder).Services;
        services.AddKafkaConsumerMiddleware<ProcessTrackingConsumerMiddleware>();
        services.AddKafkaProducerMiddleware<ProcessTrackingProducerMiddleware>();

        return new ValidProcessTrackingBuilder(services);
    }
}