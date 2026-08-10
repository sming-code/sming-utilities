namespace SmingCode.Utilities.Messaging.Config;

internal record ConsumerMiddlewareDetail(
    Type MiddlewareImplementation,
    int ProcessPosition
);
