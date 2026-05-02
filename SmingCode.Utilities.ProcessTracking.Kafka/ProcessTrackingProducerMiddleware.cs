using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ProcessTracking.Kafka;
using Config;
using Utilities.Kafka.Producers;

internal class ProcessTrackingProducerMiddleware(
    ProducerDelegate producerDelegate,
    ILogger<ProcessTrackingProducerMiddleware> _logger
)
{
    public async Task<bool> HandleAsync(
        KafkaProducerContext context,
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
            context.AddHeader(tag.Key, tag.Value.ToString()!);
        }

        return await producerDelegate(
            context
        );
    }
}
