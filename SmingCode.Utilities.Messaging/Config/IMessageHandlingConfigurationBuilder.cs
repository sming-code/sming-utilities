using Microsoft.Extensions.Configuration;

namespace SmingCode.Utilities.Messaging.Config;

public interface IMessageHandlingConfigurationBuilder
{
}

internal interface IMessageHandlingConfigurationBuilderInternal
{
    IConfiguration Configuration { get; }
    IServiceCollection Services { get; }
    bool ConsumersInitialised { get; }
    bool ProducersInitialised { get; }
    bool SetConsumersInitialised();
    bool SetProvidersInitialised();
}

