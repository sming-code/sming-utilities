namespace SmingCode.Utilities.Messaging.Kafka.Retries;
using Config;
using Consumers;
using Messaging.Consumers;

internal class RetryPreInitProcessHandler(
    IKafkaAdminClient _kafkaAdminClient
) : IKafkaConsumerPreInitProcessHandler
{
    public async Task Run(IMessagingConsumerDefinition consumerDefinition)
    {
        if (consumerDefinition.CustomPropertyHandler
            .TryGetCustomProperty<IKafkaRetryPattern>(
                Constants.RETRY_PATTERN_CUSTOM_PROPERTY_NAME,
                out var retryPattern
            ))
        {
            await _kafkaAdminClient.CreateTopic(
                $"{consumerDefinition.TopicToMatch}-retries",
                retryPattern.GetRetryDelaysInSeconds().Count
            );            
        }

        //TODO: Should we even contemplate it not working?
    }
}
