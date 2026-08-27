namespace SmingCode.Utilities.Messaging.Consumers;

internal interface IMessageConsumer
{
    Guid ConsumerId { get; }
    Task InitialiseEventConsumer(
        CancellationToken cancellationToken
    );
}
