using Microsoft.Extensions.Hosting;

namespace SmingCode.Utilities.Messaging;
using Consumers;

internal class MessagingHostedService(
    IEnumerable<IMessagingConsumerDefinition> messagingConsumerDefinitions,
    IMessageConsumerFactory messageConsumerFactory,
    HostedServiceOptions _hostedServiceOptions,
    ILogger<MessagingHostedService> _logger
) : BackgroundService
{
    private readonly List<IMessageConsumer> _messageConsumers = [
        .. messagingConsumerDefinitions
            .Select(messageConsumerFactory.GetMessageConsumer)
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _messageConsumers.ForEach(consumer => consumer.InitialiseEventConsumer(stoppingToken));
        var livenessLogInterval = _hostedServiceOptions.LivenessLogIntervalSeconds * 1000;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Message consumers running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(livenessLogInterval, stoppingToken);
        }
    }
}
