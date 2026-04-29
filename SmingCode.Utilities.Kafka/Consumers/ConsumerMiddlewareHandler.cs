namespace SmingCode.Utilities.Kafka.Consumers;

internal class ConsumerMiddlewareHandler
{
    private ConsumeDelegate _messageConsumer = null!;

    internal void SetMessageConsumer(
        ConsumeDelegate messageConsumer
    ) => _messageConsumer = messageConsumer;

    internal async Task<KafkaEventResult> RunPipeline(
        KafkaConsumerContext context
    ) => await _messageConsumer(context);
}
