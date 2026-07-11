namespace SmingCode.Utilities.Kafka.Config;

public static class MiddlewareInjection
{
    private static int _consumerMiddlewarePositionIfUnstated = 500;
    private static readonly List<Type> _consumerMiddlewareAdded = [];
    private static int _producerMiddlewarePositionIfUnstated = 500;
    private static readonly List<Type> _producerMiddlewareAdded = [];

    /// <summary>
    /// Adds middleware of type TImplementation into the kafka consumer pipeline.
    /// </summary>
    /// <typeparam name="TImplementation">The type to be added to the consumer pipeline.</typeparam>
    /// <param name="services">The IServiceCollection during service initialisation.</param>
    /// <param name="forceMiddlewarePosition">By default, consumers will be called in reverse order of that they are added.
    /// This means that the first added will be called just before the handler is called, and the last added will called immediately after event receipt.
    /// This ordering can be controlled more precisely by specifying a middleware position. The lower the number, the closer to
    /// the handler they will be called. Default positions begin at 500 and reduce by 10 each time.</param>
    /// <returns></returns>
    public static IServiceCollection AddKafkaConsumerMiddleware<TImplementation>(
        this IServiceCollection services,
        int? forceMiddlewarePosition = null
    )
    {
        var middlewareImplementationType = typeof(TImplementation);
        if (_consumerMiddlewareAdded.Contains(middlewareImplementationType))
        {
            return services;
        }

        services.AddSingleton(
            new ConsumerMiddlewareDetail(
                middlewareImplementationType,
                forceMiddlewarePosition ?? (_consumerMiddlewarePositionIfUnstated -= 10)
            )
        );

        return services;
    }

    /// <summary>
    /// Adds middleware of type TImplementation into the kafka producer pipeline.
    /// </summary>
    /// <typeparam name="TImplementation">The type to be added to the producer pipeline.</typeparam>
    /// <param name="services">The IServiceCollection during service initialisation.</param>
    /// <param name="forceMiddlewarePosition">By default, producers will be called in the order that they are added.
    /// This means that the first added will be called just before the handler is called, and the last added will called immediately after event receipt.
    /// This ordering can be controlled more precisely by specifying a middleware position. The lower the number, the
    /// earlier they will be called. Default positions begin at 500 and increase by 10 each time.</param>
    /// <returns></returns>
    public static IServiceCollection AddKafkaProducerMiddleware<TImplementation>(
        this IServiceCollection services,
        int? forceMiddlewarePosition = null
    )
    {
        var middlewareImplementationType = typeof(TImplementation);
        if (_producerMiddlewareAdded.Contains(middlewareImplementationType))
        {
            return services;
        }

        services.AddSingleton(
            new ProducerMiddlewareDetail(
                typeof(TImplementation),
                forceMiddlewarePosition ?? (_producerMiddlewarePositionIfUnstated += 10)
            )
        );

        return services;
    }
}