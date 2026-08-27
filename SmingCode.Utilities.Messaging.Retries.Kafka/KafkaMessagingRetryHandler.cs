using System.Text.Json;

namespace SmingCode.Utilities.Messaging.Retries.Kafka;
using Consumers;
using Messaging.Kafka.Consumers;
using Messaging.Kafka.Producers;

internal class KafkaMessagingRetryHandler(
    KafkaProducer _kafkaProducer
) : IMessagingRetryHandler
{
    public bool Handles(
        MessagingConsumerContext context
    ) => context is KafkaConsumerContext;

    public async Task HandleRetry(
        MessagingConsumerContext context,
        List<int> retryDelays,
        int currentRetryNo
    )
    {
        var topic = context.TopicConsumed;
        var value = context.Value;

        if (currentRetryNo >= retryDelays.Count)
        {
            await SendToDeadLetterQueue(
                topic,
                value,
                retryDelays,
                currentRetryNo
            );

            return;
        }

        var thisRetryNo = currentRetryNo + 1;
        var retryTime = DateTimeOffset.UtcNow.AddSeconds(retryDelays[currentRetryNo]);

        await _kafkaProducer.SendMessage(
            $"{topic}-retries",
            currentRetryNo.ToString(),
            value ?? string.Empty,
            new MetadataCollection
            {
                { MessagingRetryConstants.RETRY_NO_METADATA_KEY, thisRetryNo },
                { MessagingRetryConstants.RETRY_DELAYS_METADATA_KEY, retryDelays },
                { MessagingRetryConstants.RETRY_TIME_METADATA_KEY, retryTime }
            }
        );
    }

    private async Task SendToDeadLetterQueue(
        string topic,
        object? value,
        List<int> retryDelays,
        int currentRetryNo
    ) => await _kafkaProducer.SendMessage(
            $"{topic}-dlq",
            value is not null
                ? JsonSerializer.Serialize(value)
                : string.Empty,
            new MetadataCollection
            {
                { MessagingRetryConstants.RETRY_NO_METADATA_KEY, currentRetryNo },
                { MessagingRetryConstants.RETRY_DELAYS_METADATA_KEY, retryDelays }
            }
        );
}
