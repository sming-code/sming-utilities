using Confluent.Kafka.Admin;

namespace SmingCode.Utilities.Kafka;

internal class TopicManager(
    IAdminClientProvider _adminClientBuilder,
    ILogger<TopicManager> _logger
) : ITopicManager
{
    public async Task<bool> CreateTopic(
        string topicName,
        int noPartitions = -1,
        short replicationFactor = -1
    )
    {
        using var adminClient = _adminClientBuilder.GetAdminClient();

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

    public async Task<bool> RemoveTopic(string topicName)
    {
        using var adminClient = _adminClientBuilder.GetAdminClient();

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
}
