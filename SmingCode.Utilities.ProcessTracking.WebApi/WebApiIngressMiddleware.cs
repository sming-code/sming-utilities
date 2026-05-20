using Microsoft.AspNetCore.Http;
using System.Web;
using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ProcessTracking.WebApi;

using System.Net;
using Config;
using ServiceMetadata;

internal class WebApiIngressMiddleware(
    RequestDelegate _next,
    IServiceMetadataProvider serviceMetadataProvider,
    ILogger<WebApiIngressMiddleware> _logger
)
{
    private readonly Dictionary<string, object> _serviceMetadataCustomDimensions
        = serviceMetadataProvider.GetMetadata().GetCustomDimensions();

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
            var endpointTags = httpContext.GetEndpoint()
                ?.Metadata
                .OfType<TagsAttribute>()
                .SelectMany(tagAttr => tagAttr.Tags)
                .ToList() ?? [];

            var processName = endpointTags
                .FirstOrDefault(
                    tag => tag.StartsWith(Constants.PROCESS_NAME_ENDPOINT_TAG, StringComparison.InvariantCultureIgnoreCase)
                )
                ?[Constants.PROCESS_NAME_ENDPOINT_TAG.Length..];

            if (string.IsNullOrEmpty(processName))
            {
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            processTrackingHandler.InitialiseNewProcess(processName);
        }

        using var scope = _logger.BeginScope(
            processTrackingHandler.StructuredLoggingMetadata
                .Concat(_serviceMetadataCustomDimensions)
        );

        var currentProcessTags = processTrackingHandler.ProcessTags;
        httpContext.Response.OnStarting(() =>
        {
            foreach (var processTag in currentProcessTags)
            {
                httpContext.Response.Headers.TryAdd(processTag.Key, processTag.Value.ToString());
            }

            return Task.FromResult(0);
        });

        await _next(httpContext);
    }
}
