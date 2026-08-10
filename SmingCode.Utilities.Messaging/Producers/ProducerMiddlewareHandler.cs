namespace SmingCode.Utilities.Messaging.Producers;

internal class ProducerMiddlewareHandler
{
    private ProducerDelegate _messageProducer = null!;

    internal void SetMessageProducer(
        ProducerDelegate messageProducer
    ) => _messageProducer = messageProducer;

    internal async Task<bool> RunPipeline(
        MessagingProducerContext context
    ) => await _messageProducer(context);
}
