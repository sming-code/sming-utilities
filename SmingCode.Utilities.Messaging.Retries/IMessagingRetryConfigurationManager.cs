using System.Diagnostics.CodeAnalysis;

namespace SmingCode.Utilities.Messaging.Retries;

internal interface IMessagingRetryConfigurationManager
{
    bool TryGetRetryPatternForConsumer(
        Guid consumerId,
        [NotNullWhen(true)] out IMessageRetryPattern? retryPattern
    );
}
