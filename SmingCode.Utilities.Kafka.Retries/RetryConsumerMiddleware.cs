using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.Kafka.Retries;
using Config;
using Consumers;
using Producers;

internal class RetryConsumerMiddleware(
    ConsumeDelegate consumeDelegate,
    ILogger<RetryConsumerMiddleware> _logger
)
{
    public async Task<KafkaEventResult> HandleAsync(
        KafkaConsumerContext context,
        IKafkaProducer kafkaProducer
    )
    {
        if (!context.CustomPropertyHandler.TryGetCustomProperty<IKafkaRetryPattern>(
            Constants.RETRY_PATTERN_CUSTOM_PROPERTY_NAME,
            out var retryPattern
        ))
        {
            return await consumeDelegate(context);
        }

        try
        {
            return await consumeDelegate(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception occurred whilst processing kafka message - Processing retry."
            );

            var retryNo = context.Headers
                .TryGetHeader<int>(
                    KafkaRetryConstants.RETRY_NO_HEADER_NAME,
                    out var headerRetryNo
                ) ? headerRetryNo : 0;
            var retryDelays = context.Headers
                .TryGetHeader<List<int>>(
                    KafkaRetryConstants.RETRY_DELAYS_HEADER_NAME,
                    out var headerRetryDelays
                ) ? headerRetryDelays : retryPattern.GetRetryDelaysInSeconds();

            await SendRetryEvent(
                kafkaProducer,
                context.TopicConsumed,
                context.Value,
                retryDelays,
                retryNo
            );
            return KafkaEventResult.Complete;
        }
    }

    private static async Task SendRetryEvent(
        IKafkaProducer kafkaProducer,
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

        await kafkaProducer.SendEvent(
            $"{topic}-retries",
            currentRetryNo.ToString(),
            value ?? string.Empty,
            new HeaderCollection
            {
                { KafkaRetryConstants.RETRY_NO_HEADER_NAME, thisRetryNo },
                { KafkaRetryConstants.RETRY_DELAYS_HEADER_NAME, retryDelays },
                { KafkaRetryConstants.RETRY_TIME_HEADER_NAME, retryTime }
            }
        );
    }

    private static async Task SendToDeadLetterQueue(
        IKafkaProducer kafkaProducer,
        string topic,
        object? value,
        List<int> retryDelays,
        int currentRetryNo
    ) => await kafkaProducer.SendEvent(
            $"{topic}-dlq",
            value is not null
                ? JsonSerializer.Serialize(value)
                : string.Empty,
            new HeaderCollection
            {
                { KafkaRetryConstants.RETRY_NO_HEADER_NAME, currentRetryNo },
                { KafkaRetryConstants.RETRY_DELAYS_HEADER_NAME, retryDelays }
            }
        );
}
