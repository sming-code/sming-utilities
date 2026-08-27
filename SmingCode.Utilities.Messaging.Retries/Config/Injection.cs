using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SmingCode.Utilities.Messaging.Retries.Config;
using Messaging.Config;
using Messaging.Consumers;

public static class Injection
{
    private static bool _defaultRetryPatternAdded = false;

    public static IMessageHandlingConfigurationBuilder WithDefaultRetryPattern(
        this IMessageHandlingConfigurationBuilder messageHandlingConfigurationBuilder,
        IMessageRetryPattern retryPattern
    )
    {
        if (messageHandlingConfigurationBuilder is not IMessageHandlingConfigurationBuilderInternal messageHandlingConfigurationBuilderInternal)
        {
            throw new Exception();
        }

        var services = messageHandlingConfigurationBuilderInternal.Services;
        services.AddMessagingConsumerMiddleware();

        if (_defaultRetryPatternAdded)
        {
            throw new InvalidOperationException(
                "Attempt to add multiple default retry patterns to messaging configuration."
            );
        }

        services.TryAddSingleton<IMessagingRetryConfigurationManager, MessagingRetryConfigurationManager>();
        services.TryAddScoped<IMessagingRetryHandlerFactory, MessagingRetryHandlerFactory>();
        var defaultRetryDefinition = new DefaultMessageRetryDefinition(
            retryPattern
        );
        services.AddSingleton(defaultRetryDefinition);
        _defaultRetryPatternAdded = true;

        return messageHandlingConfigurationBuilder;
    }

    public static IMessagingConsumerDefinition WithRetries(
        this IMessagingConsumerDefinition consumerDefinition,
        IMessageRetryPattern retryPattern
    )
    {
        if (consumerDefinition is not IMessagingConsumerDefinitionInternal consumerDefinitionInternal)
        {
            throw new Exception();
        }

        var services = consumerDefinitionInternal.Services;
        services.AddMessagingConsumerMiddleware();

        var consumerRetryPattern = new MessageRetryDefinition(
            retryPattern,
            consumerDefinition.ConsumerId
        );
        services.AddSingleton(consumerRetryPattern);
        services.TryAddSingleton<IMessagingRetryConfigurationManager, MessagingRetryConfigurationManager>();
        services.TryAddScoped<IMessagingRetryHandlerFactory, MessagingRetryHandlerFactory>();

        return consumerDefinition;
    }

    private static IServiceCollection AddMessagingConsumerMiddleware(
        this IServiceCollection services
    ) => services.AddMessagingConsumerMiddleware<MessagingRetryConsumerMiddleware>(
        5
    );
}
