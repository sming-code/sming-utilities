namespace SmingCode.Utilities.Messaging.Retries;
using Messaging.Consumers;

internal interface IMessagingRetryHandlerFactory
{
    IMessagingRetryHandler GetMessagingRetryHandlerForContext(
        MessagingConsumerContext context
    );
}

