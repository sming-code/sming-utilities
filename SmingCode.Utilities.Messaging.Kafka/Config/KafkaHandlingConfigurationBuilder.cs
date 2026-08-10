namespace SmingCode.Utilities.Messaging.Kafka.Config;
using Consumers;
using Producers;

public interface IKafkaHandlingConfigurationBuilder
{
    IKafkaHandlingConfigurationBuilder AddKafkaConsumers();
    IKafkaHandlingConfigurationBuilder UseKafkaProducer();
}

internal interface IKafkaHandlingConfigurationBuilderInternal
{
    IServiceCollection Services { get; }
    IKafkaHandlingConfigurationBuilder WithGlobalPreInitProcessHandler<IHandler>()
        where IHandler : IKafkaConsumerPreInitProcessHandler;
}

internal class KafkaHandlingConfigurationBuilder(
    IServiceCollection services,
    TopicPartitionerFactory _topicPartitionerFactory,
    KafkaPreInitProcessHandlerOptions _preInitProcessHandlerOptions
) : IKafkaHandlingConfigurationBuilder, IKafkaHandlingConfigurationBuilderInternal
{
    public IServiceCollection Services { get; } = services;

    public IKafkaHandlingConfigurationBuilder AddKafkaConsumers()
    {
        Services.AddSingleton<IMessageConsumerFactory, KafkaConsumerFactory>();

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

        return this;
    }

    public IKafkaHandlingConfigurationBuilder WithGlobalPreInitProcessHandler<IHandler>()
        where IHandler : IKafkaConsumerPreInitProcessHandler
    {
        _preInitProcessHandlerOptions.GlobalPreInitProcessHandlers.Add(typeof(IHandler));

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

internal class KafkaPreInitProcessHandlerOptions
{
    private readonly Dictionary<Guid, List<Type>> _consumerSpecificPreInitProcessHandlers = [];
    internal List<Type> GlobalPreInitProcessHandlers { get; private set; } = [];

    internal void AddGlobalPreInitProcessHandler<IHandler>() => GlobalPreInitProcessHandlers = [
        .. GlobalPreInitProcessHandlers,
        typeof(IHandler)
    ];

    internal void AddConsumerSpecificPreInitProcessHandler<IHandler>(Guid consumerId)
    {
        if (_consumerSpecificPreInitProcessHandlers.TryGetValue(
            consumerId,
            out var consumerSpecificPreInitHandlers
        ))
        {
            consumerSpecificPreInitHandlers.Add(typeof(IHandler));
        }
        else
        {
            _consumerSpecificPreInitProcessHandlers.Add(
                consumerId,
                [ typeof(IHandler) ]
            );
        }
    }

    internal List<Type> GetConsumerSpecificPreInitProcessHandlers(
        Guid consumerId
    ) => _consumerSpecificPreInitProcessHandlers.TryGetValue(
        consumerId,
        out var matched
    ) ? matched : [];
}