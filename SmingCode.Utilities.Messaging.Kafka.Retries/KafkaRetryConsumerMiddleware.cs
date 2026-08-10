using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.Messaging.Kafka.Retries;
using Config;
using Kafka.Producers;
using Messaging.Consumers;

internal class KafkaRetryConsumerMiddleware(
    ConsumeDelegate consumeDelegate,
    ILogger<KafkaRetryConsumerMiddleware> _logger
)
{
    public async Task HandleAsync(
        MessagingConsumerContext context,
        KafkaProducer kafkaProducer
    )
    {
        if (!context.CustomPropertyHandler.TryGetCustomProperty<IKafkaRetryPattern>(
            Constants.RETRY_PATTERN_CUSTOM_PROPERTY_NAME,
            out var retryPattern
        ))
        {
            await consumeDelegate(context);

            return;
        }

        try
        {
            await consumeDelegate(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception occurred whilst processing kafka message - Processing retry."
            );

            var retryNo = context.MetadataCollection
                .TryGetMetadata<int>(
                    KafkaRetryConstants.RETRY_NO_HEADER_NAME,
                    out var metadataRetryNo
                ) ? metadataRetryNo : 0;
            var retryDelays = context.MetadataCollection
                .TryGetMetadata<List<int>>(
                    KafkaRetryConstants.RETRY_DELAYS_HEADER_NAME,
                    out var metadataRetryDelays
                ) ? metadataRetryDelays : retryPattern.GetRetryDelaysInSeconds();

            await SendRetryEvent(
                kafkaProducer,
                context.TopicConsumed,
                context.Value,
                retryDelays,
                retryNo
            );
        }
    }

    private static async Task SendRetryEvent(
        KafkaProducer kafkaProducer,
        string topic,
        object? value,
        List<int> retryDelays,
        int currentRetryNo
    )
    {
        if (currentRetryNo >= retryDelays.Count)
        {
            await SendToDeadLetterQueue(
                kafkaProducer,
                topic,
                value,
                retryDelays,
                currentRetryNo
            );

            return;
        }

        var thisRetryNo = currentRetryNo + 1;
        var retryTime = DateTimeOffset.UtcNow.AddSeconds(retryDelays[currentRetryNo]);

        await kafkaProducer.SendMessage(
            $"{topic}-retries",
            currentRetryNo.ToString(),
            value ?? string.Empty,
            new MetadataCollection
            {
                { KafkaRetryConstants.RETRY_NO_HEADER_NAME, thisRetryNo },
                { KafkaRetryConstants.RETRY_DELAYS_HEADER_NAME, retryDelays },
                { KafkaRetryConstants.RETRY_TIME_HEADER_NAME, retryTime }
            }
        );
    }

    private static async Task SendToDeadLetterQueue(
        KafkaProducer kafkaProducer,
        string topic,
        object? value,
        List<int> retryDelays,
        int currentRetryNo
    ) => await kafkaProducer.SendMessage(
            $"{topic}-dlq",
            value is not null
                ? JsonSerializer.Serialize(value)
                : string.Empty,
            new MetadataCollection
            {
                { KafkaRetryConstants.RETRY_NO_HEADER_NAME, currentRetryNo },
                { KafkaRetryConstants.RETRY_DELAYS_HEADER_NAME, retryDelays }
            }
        );
}
