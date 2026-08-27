namespace SmingCode.Utilities.Messaging.Kafka.Config;
using Consumers;
using Producers;

internal class KafkaHandlingConfigurationBuilder(
    IMessageHandlingConfigurationBuilderInternal _messageHandlingConfigurationBuilder,
    IServiceCollection services,
    TopicPartitionerFactory _topicPartitionerFactory,
    KafkaConsumerPrerequisiteOptions _preInitProcessHandlerOptions
) : IKafkaHandlingConfigurationBuilder, IKafkaHandlingConfigurationBuilderInternal
{
    public IServiceCollection Services { get; } = services;

    public IKafkaHandlingConfigurationBuilder AddKafkaConsumers()
    {
        Services.AddSingleton<IMessageConsumerFactory, KafkaConsumerFactory>();
        _messageHandlingConfigurationBuilder.SetConsumersInitialised();

        return this;
    }

    public IKafkaHandlingConfigurationBuilder UseKafkaProducer()
    {
        if (Services.Any(service => service.ServiceType == typeof(IMessagingProducer)))
        {
            throw new InvalidOperationException("There must only be one messaging producer registered.");
        }

        Services.AddScoped<IMessagingProducer, KafkaProducer>();
        Services.AddSingleton<IKafkaProducerBuilder, KafkaProducerBuilder>();
        _messageHandlingConfigurationBuilder.SetProvidersInitialised();

        return this;
    }

    public IKafkaHandlingConfigurationBuilder WithGlobalPrerequisiteHandler<IHandler>()
        where IHandler : IKafkaConsumerPreInitProcessHandler
    {
        _preInitProcessHandlerOptions.GlobalPrerequisiteHandlers.Add(typeof(IHandler));

        return this;
    }

    public IKafkaHandlingConfigurationBuilder WithTopicPartitioner(
        ITopicPartitioner topicPartitioner
    )
    {
        _topicPartitionerFactory.AddTopicPartitioner(topicPartitioner);

        return this;
    }
}

public interface IKafkaConsumerHandlingConfigurationBuilder
{
    
}

internal class KafkaConsumerHandlingConfigurationBuilder
    : IKafkaConsumerHandlingConfigurationBuilder
{
    
}

public interface IKafkaProviderHandlingConfigurationBuilder
{
    
}
