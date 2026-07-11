using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ServiceApiClient.TagApimConfiguration;
using Config;
using ServiceApiClient;

internal class ApimConfigurationMiddleware(
    SendDelegate _next,
    ApimConfigurationOptions _options,
    ILogger<ApimConfigurationMiddleware> _logger
)
{
    private const string SUBSCRIPTION_KEY_HEADER_NAME = "Ocp-Apim-Subscription-Key";
    private const string VERSION_HEADER_NAME = "X-Api-Version";
    private readonly string _optionsLogDetail
        = $"{SUBSCRIPTION_KEY_HEADER_NAME}:{_options.SubscriptionKey},{VERSION_HEADER_NAME}:{_options.TargetVersion}";

    public async Task HandleAsync(
        ApiClientSendContext context
    )
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Adding apim configuration header to http message: {ConfigurationHeaders} - {TraceType}",
                _optionsLogDetail,
                Constants.UTILITY_TRACE_TYPE
            );
        }

        context.MessageHeaders.Add(SUBSCRIPTION_KEY_HEADER_NAME, _options.SubscriptionKey);
        context.MessageHeaders.Add(VERSION_HEADER_NAME, _options.TargetVersion);

        await _next(context);
    }
}
