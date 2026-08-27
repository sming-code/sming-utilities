using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SmingCode.Utilities.Messaging.Config;
using Consumers;
using Producers;
using StartupProcesses;

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
            .SingleOrDefault(parameter => parameter.GetCustomAttribute<FromMessageBodyAttribute>() is not null)
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
        IConfiguration configuration,
        Action<IMessageHandlingConfigurationBuilder> configurationBuilder
    )
    {
        var messageHandlingConfigurationBuilder = new MessageHandlingConfigurationBuilder(
            configuration,
            services
        );

        configurationBuilder(messageHandlingConfigurationBuilder);
        
        if (messageHandlingConfigurationBuilder.ProducersInitialised)
        {
            services.AddSingleton<ProducerMiddlewareHandler>();
            services.AddScoped<IServiceInitializer, MessagingProducerMiddlewareInitialization>();
        }

        if (messageHandlingConfigurationBuilder.ConsumersInitialised)
        {
            if (!services.Any(st =>
                st.ServiceType.Name == nameof(IHostedService)
                && st.ImplementationType is not null
                && st.ImplementationType.Name == nameof(MessagingHostedService))
            )
            {
                var messagingOptions = configuration.GetRequiredSection("HostedServiceOptions")
                    .Get<HostedServiceOptions>()
                    ?? new();
                
                services.AddSingleton(messagingOptions);
                services.AddHostedService<MessagingHostedService>();
            }
            
            services.AddSingleton<ConsumerMiddlewareHandler>();
            services.AddScoped<IServiceInitializer, MessagingConsumerMiddlewareInitialization>();
        }

        return services;
    }
}
