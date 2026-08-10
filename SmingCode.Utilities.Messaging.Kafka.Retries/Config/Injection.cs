using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SmingCode.Utilities.Messaging.Kafka.Retries.Config;
using Producers;
using Messaging.Config;
using Messaging.Consumers;
using Messaging.Kafka.Config;
using StartupProcesses;

public static class Injection
{
    private static KafkaRetryOptions _kafkaRetryOptions = new();
    
    public static IKafkaHandlingConfigurationBuilder WithDefaultRetryPattern(
        this IKafkaHandlingConfigurationBuilder kafkaHandlingConfigurationBuilder,
        IKafkaRetryPattern retryPattern
    )
    {
        if (kafkaHandlingConfigurationBuilder is IKafkaHandlingConfigurationBuilderInternal kafkaHandlingConfigurationBuilderInternal)
        {
            var services = kafkaHandlingConfigurationBuilderInternal.Services;
            services.AddMessagingConsumerMiddleware<KafkaRetryConsumerMiddleware>(
                5
            );

            services.TryAddScoped<KafkaProducer>();
            services.AddScoped<IServiceInitializer, KafkaGlobalRetryInitialization>();

            kafkaHandlingConfigurationBuilderInternal.Services.TryAddSingleton<RetryPreInitProcessHandler>();
            kafkaHandlingConfigurationBuilderInternal.WithGlobalPreInitProcessHandler<RetryPreInitProcessHandler>();

            _kafkaRetryOptions.SetDefaultRetryPattern(retryPattern);
        }

        return kafkaHandlingConfigurationBuilder;
    }

    public static IKafkaConsumerOptions WithRetries(
        this IKafkaConsumerOptions consumerOptions,
        IKafkaRetryPattern retryPattern
    )
    {
        if (consumerOptions is not IKafkaConsumerOptionsInternal consumerOptionsInternal)
        {
            throw new Exception();
        }

        var consumerDefinition = consumerOptionsInternal.MessagingConsumerDefinition;
        consumerDefinition.CustomPropertyHandler
            .TryAddCustomProperty(
                Constants.RETRY_PATTERN_CUSTOM_PROPERTY_NAME,
                retryPattern
            );

        var services = consumerOptionsInternal.Services;
        services.TryAddScoped<KafkaProducer>();
        services.AddMessagingConsumerMiddleware<KafkaRetryConsumerMiddleware>(
            5
        );

        var partitioner = new DirectKeyPartitionCorrelationTopicPartitioner(
            $"{consumerDefinition.TopicToMatch}-retries"
        );
        consumerOptionsInternal.TopicPartitionerFactory.AddTopicPartitioner(
            partitioner
        );

        services.TryAddSingleton<RetryPreInitProcessHandler>();
        consumerOptionsInternal.PreInitProcessHandlerOptions.AddConsumerSpecificPreInitProcessHandler<RetryPreInitProcessHandler>(
            consumerDefinition.ConsumerId
        );

        return consumerOptions;
    }
}

internal class KafkaRetryOptions
{
    internal IKafkaRetryPattern? DefaultRetryPattern { get; private set; }
    internal Dictionary<Guid, IKafkaRetryPattern> ConsumerSpecificRetryPatterns { get; } = [];

    internal void SetDefaultRetryPattern(
        IKafkaRetryPattern defaultRetryPattern
    ) => DefaultRetryPattern = defaultRetryPattern;

    internal void AddConsumerSpecificRetryPattern(
        Guid consumerId,
        IKafkaRetryPattern consumerSpecificRetryPattern
    ) => ConsumerSpecificRetryPatterns.TryAdd(
        consumerId,
        consumerSpecificRetryPattern
    );
}