using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ProcessTracking.WebApi;
using ServiceApiClient;

internal class ProcessTrackingApiClientSendMiddleware(
    IProcessTrackingHandler _processTrackingHandler,
    ILogger<ProcessTrackingApiClientSendMiddleware> _logger
) : IApiClientSendMiddleware
{
    public async Task<TResponse> HandleAsync<TBody, TResponse>(
        ApiClientSendContext<TBody, TResponse> context,
        IApiClientSendDelegateHandler<TBody, TResponse> apiClientSendDelegate
    )
    {
        var currentProcessTags = _processTrackingHandler.ProcessTags;

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Adding process tags to http message: {ProcessTags}",
                string.Join(
                    ",",
                    currentProcessTags.Select(header =>
                        $"{header.Key}:{header.Value}"
                    )
                )
            );
        }

        foreach (var processTag in currentProcessTags)
        {
            context.MessageHeaders.Add(processTag.Key, processTag.Value.ToString()!);
        }

        return await apiClientSendDelegate.Next(context);
    }
}
