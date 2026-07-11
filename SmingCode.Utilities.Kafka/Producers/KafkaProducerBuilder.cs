namespace SmingCode.Utilities.Kafka.Producers;

internal class KafkaProducerBuilder(
    KafkaOptions _kafkaOptions,
    IEnumerable<ITopicPartitioner>? topicPartitioners
) : IKafkaProducerBuilder
{
    private readonly IProducer<string, string> _producer = GetProducerBuilder(
        _kafkaOptions,
        [.. topicPartitioners ?? []]
    );

    public IProducer<string, string> Producer => _producer;

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