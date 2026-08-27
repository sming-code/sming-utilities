using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SmingCode.Utilities.Messaging.MinimalConsumers;

public static class Injection
{
    public static IServiceCollection LoadConsumers(
        this IServiceCollection builder
    ) => builder.LoadConsumers(Assembly.GetCallingAssembly());

    public static IServiceCollection LoadConsumers<TConsumerLocator>(
        this IServiceCollection builder
    ) => builder.LoadConsumers(
        Assembly.GetAssembly(typeof(TConsumerLocator))
            ?? throw new InvalidOperationException(
                $"No loaded assembly contains the Consumer {typeof(TConsumerLocator).Name}."
            ));

    public static IServiceCollection LoadConsumers(
        this IServiceCollection builder,
        Assembly consumersAssembly
    )
    {
        var consumerImplementations = consumersAssembly
            .GetTypes()
            .Where(type =>
                typeof(IMinimalConsumer).IsAssignableFrom(type)
                && type is { IsInterface: false, IsAbstract: false }
            )
            .Select(Activator.CreateInstance)
            .OfType<IMinimalConsumer>()
            .ToList();

        consumerImplementations.ForEach(consumer =>
            consumer.Consume(builder)
        );

        return builder;
    }
}