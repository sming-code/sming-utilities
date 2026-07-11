namespace SmingCode.Utilities.Kafka;

internal class KafkaOptions
{
    public required KafkaServerOptions Server { get; set; }
    public KafkaConsumerOptions? Consumers { get; set; }
}

internal class KafkaServerOptions
{
    public required string BootstrapServers { get; set; }
    public required string SecurityProtocol { get; set; }
    public int LivenessLogIntervalSeconds { get; set; } = 30;
    public string? SaslMechanism { get; set; }
    public string? SaslUsername { get; set; }
    public string? SaslPassword { get; set; }
}

internal class KafkaConsumerOptions
{
    public bool SaveRawMessages { get; set; } = false;
    public string? RawMessageFolder { get; set; }
}