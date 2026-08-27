using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ProcessTracking.Messaging;
using Config;
using Utilities.Messaging.Producers;

internal class ProcessTrackingProducerMiddleware(
    ProducerDelegate producerDelegate,
    ILogger<ProcessTrackingProducerMiddleware> _logger
)
{
    public async Task<bool> HandleAsync(
        MessagingProducerContext context,
        IProcessTrackingHandler processTrackingHandler
    )
    {
        var processTrackingTags = processTrackingHandler.ProcessTags;

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Process tags being added to outgoing kafka message: {ProcessTagsRequired} - {TraceType}",
                string.Join(
                    ",",
                    processTrackingTags.Select(tag =>
                        $"{tag.Key}:{tag.Value}"
                    )
                ),
                Constants.PRODUCER_MIDDLEWARE_UTILITY_TRACE_TYPE
            );
        }

        foreach (var tag in processTrackingTags)
        {
            context.MetadataCollection.Add(tag.Key, tag.Value);
        }

        return await producerDelegate(
            context
        );
    }
}
