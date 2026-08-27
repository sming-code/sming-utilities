namespace SmingCode.Utilities.Messaging.Kafka.Config;

internal class KafkaConsumerPrerequisiteOptions
{
    private readonly Dictionary<Guid, List<Type>> _consumerSpecificPrerequisiteHandlers = [];
    internal List<Type> GlobalPrerequisiteHandlers { get; private set; } = [];

    internal void AddGlobalPrerequisiteHandler<IHandler>() => GlobalPrerequisiteHandlers = [
        .. GlobalPrerequisiteHandlers,
        typeof(IHandler)
    ];

    internal void AddConsumerSpecificPrerequisiteHandler<IHandler>(Guid consumerId)
    {
        if (_consumerSpecificPrerequisiteHandlers.TryGetValue(
            consumerId,
            out var consumerSpecificPrerequisiteHandlers
        ))
        {
            consumerSpecificPrerequisiteHandlers.Add(typeof(IHandler));
        }
        else
        {
            _consumerSpecificPrerequisiteHandlers.Add(
                consumerId,
                [ typeof(IHandler) ]
            );
        }
    }

    internal List<Type> GetConsumerSpecificPrerequisiteHandlers(
        Guid consumerId
    ) => _consumerSpecificPrerequisiteHandlers.TryGetValue(
        consumerId,
        out var matched
    ) ? matched : [];
}