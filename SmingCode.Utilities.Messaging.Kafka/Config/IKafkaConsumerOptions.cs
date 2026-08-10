using SmingCode.Utilities.Messaging.Kafka.Consumers;
using SmingCode.Utilities.Messaging.Kafka.Producers;

namespace SmingCode.Utilities.Messaging.Kafka.Config;

public interface IKafkaConsumerOptions
{
    
}

internal interface IKafkaConsumerOptionsInternal
{
    IServiceCollection Services { get; }
    IMessagingConsumerDefinition MessagingConsumerDefinition { get; }
    TopicPartitionerFactory TopicPartitionerFactory { get; }
    KafkaPreInitProcessHandlerOptions PreInitProcessHandlerOptions { get; }
}

internal class KafkaConsumerOptions(
    IServiceCollection _services,
    IMessagingConsumerDefinition _messagingConsumerDefinition,
    TopicPartitionerFactory _topicPartitionerFactory,
    KafkaPreInitProcessHandlerOptions _preInitProcessHandlerOptions
) : IKafkaConsumerOptions, IKafkaConsumerOptionsInternal
{
    public IServiceCollection Services => _services;
    public IMessagingConsumerDefinition MessagingConsumerDefinition => _messagingConsumerDefinition;
    public TopicPartitionerFactory TopicPartitionerFactory => _topicPartitionerFactory;
    public KafkaPreInitProcessHandlerOptions PreInitProcessHandlerOptions => _preInitProcessHandlerOptions;
}