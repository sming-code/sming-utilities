namespace SmingCode.Utilities.Kafka;

internal class AdminClientProvider(
    KafkaOptions _kafkaOptions
) : IAdminClientProvider
{
    public IAdminClient GetAdminClient()
    {
        var kafkaServerOptions = _kafkaOptions.Server;

        var adminClientConfig = new AdminClientConfig
        {
            BootstrapServers = kafkaServerOptions.BootstrapServers,
            SecurityProtocol = Enum.Parse<SecurityProtocol>(kafkaServerOptions.SecurityProtocol)
        };

        if (!string.IsNullOrEmpty(kafkaServerOptions.SaslMechanism))
        {
            adminClientConfig.SaslMechanism = Enum.Parse<SaslMechanism>(kafkaServerOptions.SaslMechanism);
            adminClientConfig.SaslUsername = kafkaServerOptions.SaslUsername;
            adminClientConfig.SaslPassword = kafkaServerOptions.SaslPassword;
        }

        return new AdminClientBuilder(
            adminClientConfig
        ).Build();
    }
}