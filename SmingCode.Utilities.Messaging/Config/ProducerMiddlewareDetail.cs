namespace SmingCode.Utilities.Messaging.Config;

internal record ProducerMiddlewareDetail(
    Type MiddlewareImplementation,
    int ProcessPosition
);
