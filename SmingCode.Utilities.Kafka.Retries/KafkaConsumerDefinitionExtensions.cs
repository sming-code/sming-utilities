using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmingCode.Utilities.Kafka.Config;

namespace SmingCode.Utilities.Kafka.Retries;
using Config;
using Consumers;
using Producers;

public static class KafkaConsumerDefinitionExtensions
{
    public static IKafkaConsumerDefinition WithRetries(
        this IKafkaConsumerDefinition consumerDefinition,
        IKafkaRetryPattern retryPattern
    )
    {
        consumerDefinition.CustomPropertyHandler
            .TryAddCustomProperty(
                Constants.RETRY_PATTERN_CUSTOM_PROPERTY_NAME,
                retryPattern
            );

        if (consumerDefinition is IKafkaConsumerDefinitionInternal consumerDefinitionInternal)
        {
            consumerDefinitionInternal.Services.AddKafkaConsumerMiddleware<RetryConsumerMiddleware>(
                5
            );
            var partitioner = new DirectKeyPartitionCorrelationTopicPartitioner(
                $"{consumerDefinition.TopicToMatch}-retries"
            );
            consumerDefinitionInternal.Services.AddSingleton<ITopicPartitioner>(partitioner);

            consumerDefinitionInternal.Services.TryAddSingleton<RetryPreInitProcessHandler>();
            consumerDefinitionInternal.WithPreInitProcessHandler<RetryPreInitProcessHandler>();
        }

        return consumerDefinition;
    }
}
