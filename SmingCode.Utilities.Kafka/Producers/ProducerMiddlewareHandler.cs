namespace SmingCode.Utilities.Kafka.Producers;

internal class ProducerMiddlewareHandler
{
    private ProducerDelegate _messageProducer = null!;

    internal void SetMessageProducer(
        ProducerDelegate messageProducer
    ) => _messageProducer = messageProducer;

    internal async Task<bool> RunPipeline(
        KafkaProducerContext context
    ) => await _messageProducer(context);
}
