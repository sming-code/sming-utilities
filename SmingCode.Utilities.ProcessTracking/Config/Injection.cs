using Microsoft.Extensions.DependencyInjection;

namespace SmingCode.Utilities.ProcessTracking.Config;
using ServiceApiClient.Config;

public static class Injection
{
    public static IServiceCollection AddProcessTracking(
        this IServiceCollection services,
        Func<IProcessTrackingBuilder, IValidProcessTrackingBuilder> initialization
    )
    {
        var processTrackingBuilder = new ProcessTrackingBuilder(services);
        initialization(processTrackingBuilder);

        services.AddScoped<IProcessTrackingHandler, ProcessTrackingHandler>();
        services.AddApiClientMiddleware<ProcessTrackingApiClientSendMiddleware>();

        return services;
    }
}
