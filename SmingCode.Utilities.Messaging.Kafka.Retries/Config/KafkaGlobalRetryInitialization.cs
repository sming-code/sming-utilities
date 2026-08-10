using Microsoft.Extensions.Hosting;

namespace SmingCode.Utilities.Messaging.Kafka.Config;
using Producers;
using Messaging.Consumers;
using StartupProcesses;

internal class KafkaGlobalRetryInitialization : IServiceInitializer
{
    public Delegate ServiceInitializer
        => (
            TopicPartitionerFactory topicPartitionerFactory,
            IEnumerable<IMessagingConsumerDefinition> messagingConsumerDefinitions
        ) =>
        {
            foreach (var consumerDefinition in messagingConsumerDefinitions)
            {
                var partitioner = new DirectKeyPartitionCorrelationTopicPartitioner(
                    $"{consumerDefinition.TopicToMatch}-retries"
                );

                topicPartitionerFactory.AddTopicPartitioner(partitioner);
            }
        };
}