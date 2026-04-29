namespace SmingCode.Utilities.Kafka.Consumers;

public delegate Task<KafkaEventResult> ConsumeDelegate(
    KafkaConsumerContext context
);
