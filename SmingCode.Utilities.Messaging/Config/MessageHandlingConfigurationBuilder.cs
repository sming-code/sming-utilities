using Microsoft.Extensions.Configuration;

namespace SmingCode.Utilities.Messaging.Config;

internal class MessageHandlingConfigurationBuilder(
    IConfiguration configuration,
    IServiceCollection services
) : IMessageHandlingConfigurationBuilder, IMessageHandlingConfigurationBuilderInternal
{
    private bool _consumersInitialised;
    private bool _producersInitialised;

    public IConfiguration Configuration { get; } = configuration;
    public IServiceCollection Services { get; } = services;
    public bool ConsumersInitialised => _consumersInitialised;
    public bool ProducersInitialised => _producersInitialised;

    public bool SetConsumersInitialised()
        => _consumersInitialised = true;

    public bool SetProvidersInitialised()
        => _producersInitialised = true;
}