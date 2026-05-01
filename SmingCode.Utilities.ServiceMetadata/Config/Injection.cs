using Microsoft.Extensions.DependencyInjection;

namespace SmingCode.Utilities.ServiceMetadata.Config;

public static class Injection
{
    public static IServiceCollection InitializeServiceMetadata(
        this IServiceCollection services
    )
    {
        services.AddSingleton<IServiceMetadataProvider, ServiceMetadataProvider>();

        return services;
    }
}
