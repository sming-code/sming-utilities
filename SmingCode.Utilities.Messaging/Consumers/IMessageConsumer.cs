namespace SmingCode.Utilities.Messaging.Consumers;

internal interface IMessageConsumer
{
    Guid ConsumerId { get; }
    void InitialiseEventConsumer(
        CancellationToken cancellationToken
    );
}
