namespace SmingCode.Utilities.Kafka.Producers;

public delegate Task<bool> ProducerDelegate(
    KafkaProducerContext context
);
