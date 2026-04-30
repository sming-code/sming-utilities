using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ProcessTracking.WebApi;
using Config;

internal class WebApiIngressMiddleware(
    RequestDelegate _next,
    ILogger<WebApiIngressMiddleware> _logger
)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        IProcessTrackingHandler processTrackingHandler
    )
    {
        var headers = httpContext.Request.Headers;

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Incoming headers are: {HeaderInfo} - {TraceType}",
                string.Join(
                    ",",
                    headers.Select(header =>
                        $"{header.Key}:{header.Value}"
                    )
                ),
                Constants.UTILITY_TRACE_TYPE
            );
        }

        if (!processTrackingHandler.TryLoadProcessDetailFromIncomingTags(
            headers.ToDictionary(
                header => header.Key,
                header => (object)header.Value
            ),
            out var processTrackingDetail
        ))
        {
            throw new Exception();
        }

        using var scope = _logger.BeginScope(
            processTrackingHandler.StructuredLoggingMetadata
        );

        var currentProcessTags = processTrackingHandler.ProcessTags;
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Process tags loaded from incoming headers: {ProcessTags} - {TraceType}",
                string.Join(
                    ",",
                    currentProcessTags.Select(header =>
                        $"{header.Key}:{header.Value}"
                    )
                ),
                Constants.UTILITY_TRACE_TYPE
            );
        }

        await _next(httpContext);
    }
}
