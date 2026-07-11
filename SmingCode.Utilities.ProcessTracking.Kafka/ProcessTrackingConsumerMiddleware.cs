using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ProcessTracking.Kafka;
using Config;
using Confluent.Kafka;
using ServiceMetadata;
using SmingCode.Utilities.Kafka;
using Utilities.Kafka.Consumers;

internal class ProcessTrackingConsumerMiddleware(
    ConsumeDelegate consumeDelegate,
    IServiceMetadataProvider? serviceMetadataProvider,
    ILogger<ProcessTrackingConsumerMiddleware> _logger
)
{
    private readonly Dictionary<string, object> _serviceMetadataCustomDimensions
        = serviceMetadataProvider?.GetMetadata().GetCustomDimensions() ?? [];

    public async Task<KafkaEventResult> HandleAsync(
        KafkaConsumerContext context,
        IProcessTrackingHandler processTrackingHandler
    )
    {
        var processTrackingHandlerConfigured =
            context.CustomPropertyHandler.TryGetCustomProperty<bool>(
                Constants.INITIALISES_PROCESS_CUSTOM_PROPERTY_NAME,
                out var initialisesProcess
            ) && initialisesProcess
                ? TryInitialiseNewProcess(
                    processTrackingHandler,
                    context.CustomPropertyHandler
                )
                : TryLoadProcessDetailFromIncomingTags(
                    context.Headers,
                    processTrackingHandler
                );

        if (!processTrackingHandlerConfigured)
        {
            return KafkaEventResult.Incomplete;
        }

        using var scope = _logger.BeginScope(
            processTrackingHandler.StructuredLoggingMetadata
                .Concat(_serviceMetadataCustomDimensions)
        );

        return await consumeDelegate(
            context
        );
    }

    private bool TryInitialiseNewProcess(
        IProcessTrackingHandler processTrackingHandler,
        ICustomPropertyHandler customPropertyHandler
    )
    {
        if (!customPropertyHandler.TryGetCustomProperty<string>(
            Constants.PROCESS_NAME_CUSTOM_PROPERTY_NAME,
            out var processName
        ) || string.IsNullOrEmpty(processName))
        {
            _logger.LogError(
                "Attempt to initialise no process, but no process name could be found."
            );

            return false;
        }

        processTrackingHandler.InitialiseNewProcess(
            processName
        );

        return true;
    }

    private bool TryLoadProcessDetailFromIncomingTags(
        HeaderCollection incomingHeaders,
        IProcessTrackingHandler processTrackingHandler
    )
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Attempting to load process details from incoming kafka message headers. Headers received are {IncomingHeaders} - {TraceType}",
                JsonSerializer.Serialize(incomingHeaders),
                Constants.CONSUMER_MIDDLEWARE_UTILITY_TRACE_TYPE
            );
        }

        if (!processTrackingHandler.TryLoadProcessDetailFromIncomingTags(
            incomingHeaders.ToDictionary(
                header => header.Key,
                header => (object)header.Value
            ),
            out _
        ))
        {
            _logger.LogError(
                "Unable to load process tracking headers. Incoming headers were {IncomingHeaders} - {TraceType}",
                JsonSerializer.Serialize(incomingHeaders),
                Constants.CONSUMER_MIDDLEWARE_UTILITY_TRACE_TYPE
            );

            return false;
        }

        return true;
    }
}
