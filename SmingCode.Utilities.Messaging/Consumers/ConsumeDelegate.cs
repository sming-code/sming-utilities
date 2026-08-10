namespace SmingCode.Utilities.Messaging.Consumers;

public delegate Task ConsumeDelegate(
    MessagingConsumerContext context
);
