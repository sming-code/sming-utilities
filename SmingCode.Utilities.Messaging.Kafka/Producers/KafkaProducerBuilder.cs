namespace SmingCode.Utilities.Messaging.Kafka.Producers;

internal class KafkaProducerBuilder(
    KafkaOptions _kafkaOptions,
    TopicPartitionerFactory topicPartitionerFactory
) : IKafkaProducerBuilder
{
    private IProducer<string, string>? _producer;

    public IProducer<string, string> Producer => _producer
        ??= GetProducerBuilder(
        _kafkaOptions,
        topicPartitionerFactory.GetTopicPartitioners()
    );

    private static IProducer<string, string> GetProducerBuilder(
        KafkaOptions kafkaOptions,
        List<ITopicPartitioner> topicPartitioners
    )
    {
        var kafkaServerOptions = kafkaOptions.Server;
        var producerBuilder = new ProducerBuilder<string, string>(
            new ProducerConfig
            {
                BootstrapServers = kafkaServerOptions.BootstrapServers,
                SecurityProtocol = Enum.Parse<SecurityProtocol>(kafkaServerOptions.SecurityProtocol),
                ApiVersionRequest = false,
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000,
                Acks = Acks.All
            });

        topicPartitioners.ForEach(topicPartitioner =>
             producerBuilder = topicPartitioner.GetPartitionedProducerBuilder(producerBuilder)
        );

        return producerBuilder.Build();
    }
}
