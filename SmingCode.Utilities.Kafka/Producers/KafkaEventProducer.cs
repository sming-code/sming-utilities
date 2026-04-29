namespace SmingCode.Utilities.Kafka.Producers;

internal class KafkaProducer(
    KafkaServerOptions _kafkaServerOptions,
    ProducerMiddlewareHandler _producerMiddlewareHandler,
    IServiceProvider _serviceProvider
) : IKafkaProducer
{
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
    ) where TValue : notnull => await ProcessKafkaEvent(
        topic,
        key,
        value
    );

    public async Task<bool> SendEvent<TValue>(
        string topic,
        TValue value,
        Headers headers
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
        Headers headers
    ) where TValue : notnull => await ProcessKafkaEvent(
        topic,
        key,
        value,
        headers
    );

    private async Task<bool> ProcessKafkaEvent<TKey, TValue>(
        string topic,
        TKey? key,
        TValue value,
        Headers? headers = null
    ) where TValue : notnull
    {

        Func<KafkaProducerContext, Task<bool>> produceDelegate = async (context) =>
        {
            using var producer = BuildProducer<TKey, TValue>();
            
            var message = new Message<TKey, TValue>
            {
                Value = (TValue)context.Value,
                Headers = context.Headers
            };

            if (typeof(TKey) != typeof(Null) && context.HasKey)
            {
                message.Key = (TKey)context.Key;
            }

            var deliveryResult = await producer.ProduceAsync(
                context.Topic,
                message
            );

            return deliveryResult.Status == PersistenceStatus.Persisted;
        };

        var kafkaProducerContext = new KafkaProducerContext(
            topic,
            key,
            typeof(TKey),
            value,
            typeof(TValue),
            headers ?? [],
            produceDelegate,
            _serviceProvider
        ).AddHeader("message-identifier", Guid.NewGuid().ToString());

        return await _producerMiddlewareHandler.RunPipeline(kafkaProducerContext);
    }

    private IProducer<TKey, TValue> BuildProducer<TKey, TValue>()
    {
        var producerBuilder = new ProducerBuilder<TKey, TValue>(
            new ProducerConfig
            {
                BootstrapServers = _kafkaServerOptions.BootstrapServers,
                ApiVersionRequest = false,
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000,
                Acks = Acks.All
            });

        producerBuilder.SetKeySerializer(KafkaSerializerFactory.GetSerializer<TKey>());
        producerBuilder.SetValueSerializer(KafkaSerializerFactory.GetSerializer<TValue>());

        return producerBuilder.Build();
    }
}
