using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ProcessTracking.Kafka;
using Config;
using ServiceMetadata;
using Utilities.Kafka.Consumers;

internal class ProcessTrackingConsumerMiddleware(
    ConsumeDelegate consumeDelegate,
    IServiceMetadataProvider serviceMetadataProvider,
    ILogger<ProcessTrackingConsumerMiddleware> _logger
)
{
    private readonly Dictionary<string, object> _serviceMetadataCustomDimensions
        = serviceMetadataProvider.GetMetadata().GetCustomDimensions();

    public async Task<KafkaEventResult> HandleAsync(
        KafkaConsumerContext context,
        IProcessTrackingHandler processTrackingHandler
    )
    {
        var messageHeaders = context.Headers
            .ToDictionary(
                header => header.Key,
                header => Encoding.UTF8.GetString(header.GetValueBytes())
            );

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Attempting to load process details from incoming kafka message headers. Headers received are {IncomingHeaders} - {TraceType}",
                JsonSerializer.Serialize(messageHeaders),
                Constants.CONSUMER_MIDDLEWARE_UTILITY_TRACE_TYPE
            );
        }

        if (!processTrackingHandler.TryLoadProcessDetailFromIncomingTags(
            messageHeaders.ToDictionary(
                header => header.Key,
                header => (object)header.Value
            ),
            out _
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
                .Concat(_serviceMetadataCustomDimensions)
        );

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Process tracking details loaded from incoming message headers - {TraceType}",
                Constants.CONSUMER_MIDDLEWARE_UTILITY_TRACE_TYPE
            );
        }

        return await consumeDelegate(
            context
        );
    }
}
