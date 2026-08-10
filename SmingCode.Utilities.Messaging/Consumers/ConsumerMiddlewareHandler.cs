namespace SmingCode.Utilities.Messaging.Consumers;

internal class ConsumerMiddlewareHandler
{
    private ConsumeDelegate _messageConsumer = null!;

    internal void SetMessageConsumer(
        ConsumeDelegate messageConsumer
    ) => _messageConsumer = messageConsumer;

    internal async Task RunPipeline(
        MessagingConsumerContext context
    ) => await _messageConsumer(context);
}
