using Microsoft.Extensions.Configuration;
using SmingCode.Utilities.Messaging.Kafka.Producers;

namespace SmingCode.Utilities.Messaging.Kafka.Config;

public static class Injection
{
    private static readonly TopicPartitionerFactory _topicPartitionerFactory = new();
    private static readonly KafkaPreInitProcessHandlerOptions _preInitHandlerOptions = new();

    public static IMessageHandlingConfigurationBuilder UseKafka(
        this IMessageHandlingConfigurationBuilder configurationBuilder,
        IConfiguration configuration,
        Action<IKafkaHandlingConfigurationBuilder> kafkaConfigurationBuilder
    )
    {
        if (configurationBuilder is not MessageHandlingConfigurationBuilder concreteConfigurationBuilder)
        {
            throw new Exception();
        }

        var services = concreteConfigurationBuilder.Services;

        var kafkaOptions = configuration.GetRequiredSection("Kafka")
            .Get<KafkaOptions>()
            ?? throw new InvalidOperationException("No valid kafka configuration section found.");
        services.AddSingleton(kafkaOptions);
        services.AddSingleton<IKafkaAdminClient, KafkaAdminClient>();

        services.AddSingleton(_preInitHandlerOptions);
        services.AddSingleton(_topicPartitionerFactory);

        var kafkaHandlingConfigurationBuilder = new KafkaHandlingConfigurationBuilder(
            services,
            _topicPartitionerFactory,
            _preInitHandlerOptions
        );
        kafkaConfigurationBuilder(kafkaHandlingConfigurationBuilder);

        return configurationBuilder;
    }

    public static IMessagingConsumerDefinition WithKafkaOptions(
        this IMessagingConsumerDefinition messagingConsumerDefinition,
        Action<IKafkaConsumerOptions> kafkaOptions
    )
    {
        if (messagingConsumerDefinition is not IMessagingConsumerDefinitionInternal messagingConsumerDefinitionInternal)
        {
            throw new Exception();
        }

        var kafkaMessageOptions = new KafkaConsumerOptions(
            messagingConsumerDefinitionInternal.Services,
            messagingConsumerDefinition,
            _topicPartitionerFactory,
            _preInitHandlerOptions
        );

        kafkaOptions(kafkaMessageOptions);
        return messagingConsumerDefinition;
    }
}
