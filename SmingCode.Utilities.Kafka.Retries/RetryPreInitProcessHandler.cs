namespace SmingCode.Utilities.Kafka.Retries;
using Config;
using Consumers;

internal class RetryPreInitProcessHandler(
    ITopicManager _topicManager
) : IKafkaConsumerPreInitProcessHandler
{
    public async Task Run(IKafkaConsumerDefinition consumerDefinition)
    {
        if (consumerDefinition.CustomPropertyHandler
            .TryGetCustomProperty<IKafkaRetryPattern>(
                Constants.RETRY_PATTERN_CUSTOM_PROPERTY_NAME,
                out var retryPattern
            ))
        {
            await _topicManager.CreateTopic(
                $"{consumerDefinition.TopicToMatch}-retries",
                retryPattern.GetRetryDelaysInSeconds().Count
            );            
        }

        //TODO: Should we even contemplate it not working?
    }
}
