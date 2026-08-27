using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SmingCode.Utilities.Messaging.Retries.Kafka.Config;
using Messaging.Kafka.Producers;
using Messaging.Kafka.Config;
using StartupProcesses;

public static class Injection
{
    public static IKafkaHandlingConfigurationBuilder IncludeInRetries(
        this IKafkaHandlingConfigurationBuilder kafkaHandlingConfigurationBuilder
    )
    {
        if (kafkaHandlingConfigurationBuilder is IKafkaHandlingConfigurationBuilderInternal kafkaHandlingConfigurationBuilderInternal)
        {
            var services = kafkaHandlingConfigurationBuilderInternal.Services;
            services.AddScoped<IMessagingRetryHandler, KafkaMessagingRetryHandler>();

            services.TryAddScoped<KafkaProducer>();
            services.AddScoped<IServiceInitializer, KafkaRetryInitialization>();
        }

        return kafkaHandlingConfigurationBuilder;
    }
}
