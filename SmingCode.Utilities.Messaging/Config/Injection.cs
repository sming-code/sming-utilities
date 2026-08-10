using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace SmingCode.Utilities.Messaging.Config;
using Consumers;
using Producers;
using SmingCode.Utilities.StartupProcesses;

public static class Injection
{
    public static IMessagingConsumerDefinition MapConsumer(
        this IServiceCollection services,
        string topicToMatch,
        Delegate handler
    )
    {
        var handlerMethodParameters = handler.Method.GetParameters();
        var consumerBodyType = handlerMethodParameters
            .SingleOrDefault(parameter => parameter.GetCustomAttribute<FromBodyAttribute>() is not null)
                ?.ParameterType
                ?? typeof(string);

        var consumerDefinitionType = typeof(MessagingConsumerDefinition<>);
        var typedConsumerDefinitionType = consumerDefinitionType
            .MakeGenericType(consumerBodyType);

        var newConsumerDefinition = (IMessagingConsumerDefinition)Activator.CreateInstance(
            typedConsumerDefinitionType,
            [ topicToMatch, handler, services ]
        )!;
        services.AddSingleton(newConsumerDefinition);

        return newConsumerDefinition;
    }

    public static IServiceCollection InitializeMessageHandling(
        this IServiceCollection services,
        bool includeConsumers,
        Action<IMessageHandlingConfigurationBuilder> configurationBuilder
    )
    {
        var messageHandlingConfigurationBuilder = new MessageHandlingConfigurationBuilder(
            services
        );

        configurationBuilder(messageHandlingConfigurationBuilder);
        
        services.AddSingleton<ProducerMiddlewareHandler>();
        services.AddScoped<IServiceInitializer, MessagingProducerMiddlewareInitialization>();

        if (includeConsumers)
        {
            services.AddSingleton<ConsumerMiddlewareHandler>();
            services.AddScoped<IServiceInitializer, MessagingConsumerMiddlewareInitialization>();
        }

        return services;
    }
}
