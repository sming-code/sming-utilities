using System.Text.Json;

namespace SmingCode.Utilities.Messaging.Kafka.Producers;

internal class KafkaProducer(
    IKafkaProducerBuilder _kafkaProducerBuilder,
    ProducerMiddlewareHandler _producerMiddlewareHandler,
    IServiceProvider _serviceProvider
) : IMessagingProducer, IMessagingProducerWithKeyValues
{
    public async Task<bool> SendMessage(
        string topic
    ) => await ProcessKafkaEvent<Null, Null>(
        topic,
        null,
        null
    );

    public async Task<bool> SendMessage<TBody>(
        string topic,
        TBody value
    ) where TBody : notnull => await ProcessKafkaEvent<Null, TBody>(
        topic,
        null,
        value
    );

    public async Task<bool> SendMessage(
        string topic,
        MetadataCollection? metadataCollection = null
    ) => await ProcessKafkaEvent<Null, Null>(
        topic,
        null,
        null,
        metadataCollection
    );

    public async Task<bool> SendMessage<TValue>(
        string topic,
        TValue value,
        MetadataCollection? metadataCollection = null
    ) where TValue : notnull => await ProcessKafkaEvent<Null, TValue>(
        topic,
        null,
        value,
        metadataCollection
    );

    public async Task<bool> SendMessage<TKey, TBody>(
        string topic,
        TKey key,
        TBody value
    ) where TKey : notnull
      where TBody : notnull
      => await ProcessKafkaEvent<TKey, TBody>(
        topic,
        key,
        value
    );

    public async Task<bool> SendMessage<TKey, TBody>(
        string topic,
        TKey key,
        TBody value,
        MetadataCollection? metadataCollection = null
    ) where TKey : notnull
      where TBody : notnull
      => await ProcessKafkaEvent<TKey, TBody>(
            topic,
            key,
            value,
            metadataCollection
        );

    protected async Task<bool> ProcessKafkaEvent<TKey, TValue>(
        string topic,
        object? key,
        object? value,
        MetadataCollection? metadataCollection = null
    ) where TKey : notnull where TValue : notnull
    {
        async Task<bool> produceDelegate(MessagingProducerContext context)
        {
            var message = new Message<string, string>
            {
                Headers = context.MetadataCollection.ToKafkaHeaders(),
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

        var kafkaProducerContext = new MessagingProducerContext(
            topic,
            value,
            typeof(TValue),
            metadataCollection ?? [],
            produceDelegate,
            _serviceProvider
        );
        kafkaProducerContext.MetadataCollection.Add("message-identifier", Guid.NewGuid().ToString());

        return await _producerMiddlewareHandler.RunPipeline(kafkaProducerContext);
    }
}
