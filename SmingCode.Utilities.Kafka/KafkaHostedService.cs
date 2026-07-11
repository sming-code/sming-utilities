using Microsoft.Extensions.Hosting;

namespace SmingCode.Utilities.Kafka;
using Consumers;

internal class KafkaHostedService(
    IEnumerable<IKafkaConsumerDefinition> kafkaConsumerDefinitions,
    KafkaOptions _kafkaOptions,
    IServiceProvider serviceProvider,
    ILogger<KafkaHostedService> _logger
) : BackgroundService
{
    private readonly List<IKafkaConsumer> _kafkaConsumers = [.. kafkaConsumerDefinitions
        .Select(definition =>
        {
            var consumerType = typeof(KafkaConsumer<,>)
                .MakeGenericType(definition.GetType().GetGenericArguments());

            return (IKafkaConsumer)ActivatorUtilities.CreateInstance(
                serviceProvider,
                consumerType,
                [ definition ]
            );
        })];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _kafkaConsumers.ForEach(consumer => consumer.InitialiseEventConsumer(stoppingToken));
        var livenessLogInterval = _kafkaOptions.Server.LivenessLogIntervalSeconds * 1000;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Kafka consumers running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(livenessLogInterval, stoppingToken);
        }
    }
}
