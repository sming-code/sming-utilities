using Microsoft.Extensions.DependencyInjection;

namespace SmingCode.Utilities.ProcessTracking.WebApi.Config;
using StartupProcesses;

public static class Injection
{
    public static IValidProcessTrackingBuilder AddApiMiddleware(
        this IProcessTrackingBuilder builder
    )
    {
        var services = ((IProcessTrackingBuilderInternal)builder).Services;
        services.AddScoped<IServiceInitializer, ProcessTrackingInitialization>();

        return new ValidProcessTrackingBuilder(services);
    }
}
