using System.Text;
using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ProcessTracking.Kafka;

using System.Text.Json;
using Config;
using ServiceMetadata;
using Utilities.Kafka.Consumers;

internal class ProcessTrackingConsumerMiddleware(
    ConsumeDelegate consumeDelegate,
    ILogger<ProcessTrackingConsumerMiddleware> _logger
)
{
    public async Task<KafkaEventResult> HandleAsync(
        KafkaConsumerContext context,
        IProcessTrackingHandler processTrackingHandler,
        IServiceMetadataProvider serviceMetadataProvider
    )
    {
        var messageHeaders = context.Headers
            .ToDictionary(
                header => header.Key,
                header => Encoding.UTF8.GetString(header.GetValueBytes())
            );

        if (!processTrackingHandler.TryLoadProcessDetailFromIncomingTags(
            messageHeaders.ToDictionary(
                header => header.Key,
                header => (object)header.Value
            ),
            out var processTrackingDetail
        ))
        {
            _logger.LogError(
                "Unable to load process tracking headers. Incoming headers were {IncomingHeaders} - {TraceType}",
                JsonSerializer.Serialize(context.Headers),
                Constants.CONSUMER_MIDDLEWARE_UTILITY_TRACE_TYPE
            );

            return KafkaEventResult.Incomplete;
        }

        using var scope = _logger.BeginScope(
            processTrackingHandler.StructuredLoggingMetadata
                .Concat(serviceMetadataProvider.GetMetadata().GetCustomDimensions())
        );

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Process tracking details loaded from incoming message headers - {TraceType}",
                Constants.CONSUMER_MIDDLEWARE_UTILITY_TRACE_TYPE
            );
            _logger.LogInformation(
                "Process tracking details loaded from incoming message headers."
            );
        }

        return await consumeDelegate(
            context
        );
    }
}
