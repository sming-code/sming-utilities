using Microsoft.Extensions.DependencyInjection;

namespace SmingCode.Utilities.ProcessTracking.WebApi;
using ServiceApiClient.Config;
using StartupProcesses;

public static class Injection
{
    public static IValidProcessTrackingBuilder AddApiMiddleware(
        this IProcessTrackingBuilder builder
    )
    {
        var services = ((IProcessTrackingBuilderInternal)builder).Services;
        services.AddScoped<IServiceInitializer, ProcessTrackingInitialization>();
        services.AddApiClientMiddleware<ProcessTrackingApiClientSendMiddleware>();

        return new ValidProcessTrackingBuilder(services);
    }
}
