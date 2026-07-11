using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace SmingCode.Utilities.Kafka.Config;
using Consumers;
using Producers;
using SmingCode.Utilities.StartupProcesses;

public static class Injection
{
    public static IKafkaConsumerDefinition MapConsumer(
        this IServiceCollection services,
        string topicToMatch,
        Delegate handler
    )
    {
        var handlerMethodParameters = handler.Method.GetParameters();
        var consumerKeyType = handlerMethodParameters
            .SingleOrDefault(parameter => parameter.GetCustomAttribute<FromEventKeyAttribute>() is not null)
                ?.ParameterType
                ?? typeof(Ignore);
        var consumerValueType = handlerMethodParameters
            .SingleOrDefault(parameter => parameter.GetCustomAttribute<FromEventValueAttribute>() is not null)
                ?.ParameterType
                ?? typeof(Ignore);

        var kafkaConsumerDefinitionType = typeof(KafkaConsumerDefinition<,>);
        var typedKafkaConsumerDefinitionType = kafkaConsumerDefinitionType
            .MakeGenericType(consumerKeyType, consumerValueType);

        var newKafkaConsumerDefinition = (IKafkaConsumerDefinition)Activator.CreateInstance(
            typedKafkaConsumerDefinitionType,
            [ topicToMatch, handler, services ]
        )!;
        services.AddSingleton(newKafkaConsumerDefinition);

        return newKafkaConsumerDefinition;
    }

    public static IServiceCollection InitializeKafkaHandling(
        this IServiceCollection services,
        IConfiguration configuration,
        bool includeConsumers = false
    )
    {
        var kafkaOptions = configuration.GetRequiredSection("Kafka")
            .Get<KafkaOptions>()
            ?? throw new InvalidOperationException("No valid kafka configuration section found.");
        services.AddSingleton(kafkaOptions);
        services.AddSingleton<IAdminClientProvider, AdminClientProvider>();
        services.AddSingleton<ITopicManager, TopicManager>();
        services.AddScoped<IKafkaProducer, KafkaProducer>();
        services.AddSingleton<IKafkaProducerBuilder, KafkaProducerBuilder>();

        services.AddSingleton<ProducerMiddlewareHandler>();
        services.AddScoped<IServiceInitializer, KafkaProducerMiddlewareInitialization>();

        if (includeConsumers)
        {
            services.AddHostedService<KafkaHostedService>();
            services.AddSingleton<ConsumerMiddlewareHandler>();
            services.AddScoped<IServiceInitializer, KafkaConsumerMiddlewareInitialization>();
        }

        return services;
    }
}