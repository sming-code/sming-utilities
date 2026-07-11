namespace SmingCode.Utilities.Kafka.Config;

internal record ProducerMiddlewareDetail(
    Type MiddlewareImplementation,
    int ProcessPosition
);
