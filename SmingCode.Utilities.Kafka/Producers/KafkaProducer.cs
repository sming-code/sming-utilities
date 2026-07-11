using System.Text.Json;

namespace SmingCode.Utilities.Kafka.Producers;

internal class KafkaProducer(
    IKafkaProducerBuilder _kafkaProducerBuilder,
    ProducerMiddlewareHandler _producerMiddlewareHandler,
    IServiceProvider _serviceProvider
) : IKafkaProducer
{
    public async Task<bool> SendEvent(
        string topic
    ) => await ProcessKafkaEvent<Null, Null>(
        topic,
        null,
        null
    );

    public async Task<bool> SendEvent<TValue>(
        string topic,
        TValue value
    ) where TValue : notnull => await ProcessKafkaEvent<Null, TValue>(
        topic,
        null,
        value
    );

    public async Task<bool> SendEvent<TKey, TValue>(
        string topic,
        TKey key,
        TValue value
    ) where TKey : notnull
      where TValue : notnull
      => await ProcessKafkaEvent<TKey, TValue>(
        topic,
        key,
        value
    );

    public async Task<bool> SendEvent(
        string topic,
        HeaderCollection headers
    ) => await ProcessKafkaEvent<Null, Null>(
        topic,
        null,
        null,
        headers
    );

    public async Task<bool> SendEvent<TValue>(
        string topic,
        TValue value,
        HeaderCollection headers
    ) where TValue : notnull => await ProcessKafkaEvent<Null, TValue>(
        topic,
        null,
        value,
        headers
    );

    public async Task<bool> SendEvent<TKey, TValue>(
        string topic,
        TKey key,
        TValue value,
        HeaderCollection headers
    ) where TKey : notnull
      where TValue : notnull
      => await ProcessKafkaEvent<TKey, TValue>(
            topic,
            key,
            value,
            headers
        );

    protected async Task<bool> ProcessKafkaEvent<TKey, TValue>(
        string topic,
        object? key,
        object? value,
        HeaderCollection? headers = null
    ) where TKey : notnull where TValue : notnull
    {
        async Task<bool> produceDelegate(KafkaProducerContext context)
        {
            var message = new Message<string, string>
            {
                Headers = context.Headers.ToKafkaHeaders(),
                Key = typeof(TKey) == typeof(Null) || key is null
                    ? string.Empty
                    : typeof(TKey) == typeof(string)
                        ? (string)key
                        : JsonSerializer.Serialize(key),
                Value = typeof(TValue) == typeof(Null) || value is null
                    ? string.Empty
                    : typeof(TValue) == typeof(string)
                        ? (string)value
                        : JsonSerializer.Serialize(value)
            };

            var deliveryResult = await _kafkaProducerBuilder.Producer.ProduceAsync(
                context.Topic,
                message
            );

            return deliveryResult.Status == PersistenceStatus.Persisted;
        }

        var kafkaProducerContext = new KafkaProducerContext(
            topic,
            key,
            typeof(TKey),
            value,
            typeof(TValue),
            headers ?? [],
            produceDelegate,
            _serviceProvider
        );
        kafkaProducerContext.Headers.Add("message-identifier", Guid.NewGuid().ToString());

        return await _producerMiddlewareHandler.RunPipeline(kafkaProducerContext);
    }
}
