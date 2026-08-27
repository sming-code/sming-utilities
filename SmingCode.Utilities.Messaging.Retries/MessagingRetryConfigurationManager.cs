using System.Diagnostics.CodeAnalysis;

namespace SmingCode.Utilities.Messaging.Retries;
using Messaging.Consumers;

internal class MessagingRetryConfigurationManager(
    DefaultMessageRetryDefinition? defaultMessageRetryDefinition,
    IEnumerable<MessageRetryDefinition> messageRetryDefinitions,
    IEnumerable<IMessagingConsumerDefinition> messagingConsumerDefinitions
) : IMessagingRetryConfigurationManager
{
    private readonly Dictionary<Guid, IMessageRetryPattern?> _consumerRetryDefinitions
        = messagingConsumerDefinitions
            .ToDictionary(
                consumerDefinition => consumerDefinition.ConsumerId,
                consumerDefinition => messageRetryDefinitions.SingleOrDefault(
                    retryDefinition => retryDefinition.ConsumerId == consumerDefinition.ConsumerId
                )?.MessageRetryPattern ?? defaultMessageRetryDefinition?.MessageRetryPattern
            );

    public bool TryGetRetryPatternForConsumer(
        Guid consumerId,
        [NotNullWhen(true)] out IMessageRetryPattern? retryPattern
    ) => _consumerRetryDefinitions.TryGetValue(consumerId, out retryPattern);
}