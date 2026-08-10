using Confluent.Kafka.Admin;

namespace SmingCode.Utilities.Messaging.Kafka;

internal class KafkaAdminClient(
    KafkaOptions _kafkaOptions,
    ILogger<KafkaAdminClient> _logger
) : IKafkaAdminClient
{
    public async Task<bool> CreateTopic(
        string topicName,
        int noPartitions = 1,
        short replicationFactor = 1
    )
    {
        using var adminClient = GetAdminClient();

        try
        {
            await adminClient.CreateTopicsAsync(
            [
                new() {
                    Name = topicName,
                    NumPartitions = noPartitions,
                    ReplicationFactor = replicationFactor
                }
            ]);

            return true;
        }
        catch (CreateTopicsException ex)
        {
            _logger.LogError(ex, "Unable to add topic {topicName} to kafka.", topicName);

            return false;
        }
    }

    public async Task<bool> RemoveTopic(
        string topicName
    )
    {
        using var adminClient = GetAdminClient();

        try
        {
            await adminClient.DeleteTopicsAsync(
                [
                    topicName
                ],
                new DeleteTopicsOptions { OperationTimeout = TimeSpan.FromSeconds(2) }
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to remove topic {topicName}.", topicName);
            return false;
        }
    }

    private IAdminClient GetAdminClient()
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