using Microsoft.Extensions.DependencyInjection;

namespace SmingCode.Utilities.ServiceMetadata.Config;
using StartupProcesses;

public static class Injection
{
    public static IServiceCollection InitializeServiceMetadata(
        this IServiceCollection services
    )
    {
        services.AddSingleton<IServiceMetadataProvider, ServiceMetadataProvider>();
        services.AddScoped<IServiceInitializer, ServiceMetadataLoggingScopeInitialization>();

        return services;
    }
}
