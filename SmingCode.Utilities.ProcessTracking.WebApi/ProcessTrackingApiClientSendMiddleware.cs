using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ProcessTracking.WebApi;
using Config;
using ServiceApiClient;

internal class ProcessTrackingApiClientSendMiddleware(
    SendDelegate sendDelegate,
    ILogger<ProcessTrackingApiClientSendMiddleware> _logger
)
{
    public async Task HandleAsync(
        ApiClientSendContext context,
        IProcessTrackingHandler _processTrackingHandler
    )
    {
        var currentProcessTags = _processTrackingHandler.ProcessTags;

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Adding process tags to http message: {ProcessTags} - {TraceType}",
                string.Join(
                    ",",
                    currentProcessTags.Select(header =>
                        $"{header.Key}:{header.Value}"
                    )
                ),
                Constants.UTILITY_TRACE_TYPE
            );
        }

        foreach (var processTag in currentProcessTags)
        {
            context.MessageHeaders.Add(processTag.Key, processTag.Value.ToString()!);
        }

        await sendDelegate(context);
    }
}
