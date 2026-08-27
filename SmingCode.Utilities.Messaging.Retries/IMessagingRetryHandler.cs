namespace SmingCode.Utilities.Messaging.Retries;
using Messaging.Consumers;

internal interface IMessagingRetryHandler
{
    bool Handles(
        MessagingConsumerContext context
    );
    Task HandleRetry(
        MessagingConsumerContext context,
        List<int> retryDelays,
        int currentRetryNo
    );
}

