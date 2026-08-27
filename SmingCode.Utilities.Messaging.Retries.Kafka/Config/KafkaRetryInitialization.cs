namespace SmingCode.Utilities.Messaging.Retries.Kafka.Config;
using Messaging.Kafka.Producers;
using Messaging.Consumers;
using Messaging.Kafka;
using StartupProcesses;

internal class KafkaRetryInitialization : IServiceInitializer
{
    public Delegate ServiceInitializer
        => async (
            TopicPartitionerFactory topicPartitionerFactory,
            IKafkaAdminClient kafkaAdminClient,
            IMessagingRetryConfigurationManager messagingRetryConfigurationManager,
            IEnumerable<IMessagingConsumerDefinition> messagingConsumerDefinitions
        ) =>
        {
            var consumersWithRetryDefinitions = messagingConsumerDefinitions
                .Select(consumerDefinition => new
                {
                    consumerDefinition.TopicToMatch,
                    RetryPattern = messagingRetryConfigurationManager.TryGetRetryPatternForConsumer(
                        consumerDefinition.ConsumerId,
                        out var retryPattern
                    ) ? retryPattern : null
                })
                .Where(consumerDetail => consumerDetail.RetryPattern != null);

            foreach (var definition in consumersWithRetryDefinitions)
            {
                var partitioner = new DirectKeyPartitionCorrelationTopicPartitioner(
                    $"{definition.TopicToMatch}-retries"
                );
                topicPartitionerFactory.AddTopicPartitioner(partitioner);

                await kafkaAdminClient.CreateTopic(
                    $"{definition.TopicToMatch}-retries",
                    definition.RetryPattern!.GetRetryDelaysInSeconds().Count
                );            
            }
        };
}
