namespace SmingCode.Utilities.Messaging.Retries;
using Messaging.Consumers;

internal class MessagingRetryHandlerFactory(
    IEnumerable<IMessagingRetryHandler> _messagingRetryHandlers
) : IMessagingRetryHandlerFactory
{
    public IMessagingRetryHandler GetMessagingRetryHandlerForContext(
        MessagingConsumerContext context
    ) => _messagingRetryHandlers.FirstOrDefault(retryHandler => retryHandler.Handles(context))
        ?? throw new NotSupportedException("A context has been passed for which no retry handler could be found.");
}

