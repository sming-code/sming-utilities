namespace SmingCode.Utilities.Messaging.Producers;

public delegate Task<bool> ProducerDelegate(
    MessagingProducerContext context
);
