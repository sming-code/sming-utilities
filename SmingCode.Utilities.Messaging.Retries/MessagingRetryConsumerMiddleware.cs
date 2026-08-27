using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.Messaging.Retries;
using Messaging.Consumers;

internal class MessagingRetryConsumerMiddleware(
    ConsumeDelegate consumeDelegate,
    IMessagingRetryConfigurationManager _messagingRetryConfigurationManager,
    ILogger<MessagingRetryConsumerMiddleware> _logger
)
{
    public async Task HandleAsync(
        MessagingConsumerContext context,
        IMessagingRetryHandlerFactory messagingRetryHandlerFactory
    )
    {
        if (!_messagingRetryConfigurationManager.TryGetRetryPatternForConsumer(
            context.Consumer.ConsumerId,
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
                    MessagingRetryConstants.RETRY_NO_METADATA_KEY,
                    out var metadataRetryNo
                ) ? metadataRetryNo : 0;
            var retryDelays = context.MetadataCollection
                .TryGetMetadata<List<int>>(
                    MessagingRetryConstants.RETRY_DELAYS_METADATA_KEY,
                    out var metadataRetryDelays
                ) ? metadataRetryDelays : retryPattern.GetRetryDelaysInSeconds();
            var retryHandler = messagingRetryHandlerFactory.GetMessagingRetryHandlerForContext(
                context
            );

            await retryHandler.HandleRetry(
                context,
                retryDelays,
                retryNo
            );
        }
    }
}