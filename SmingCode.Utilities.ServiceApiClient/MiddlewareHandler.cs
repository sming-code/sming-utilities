namespace SmingCode.Utilities.ServiceApiClient;

internal class MiddlewareHandler
{
    private SendDelegate _messageSender = null!;

    internal void SetMessageSender(
        SendDelegate messageSender
    ) => _messageSender = messageSender;

    internal async Task RunPipeline(
        ApiClientSendContext context
    ) => await _messageSender(context);
}
