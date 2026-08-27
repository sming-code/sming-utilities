namespace SmingCode.Utilities.Messaging.Retries;

public interface IMessageRetryPattern
{
    List<int> GetRetryDelaysInSeconds();
}

internal record MessageRetryDefinition(
    IMessageRetryPattern MessageRetryPattern,
    Guid ConsumerId
);

internal record DefaultMessageRetryDefinition(
    IMessageRetryPattern MessageRetryPattern
);