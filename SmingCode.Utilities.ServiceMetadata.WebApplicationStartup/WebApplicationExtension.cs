using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SmingCode.Utilities.ServiceMetadata.WebApplicationStartup;

public static class WebApplicationExtension
{
    public static void RunWithServiceMetadataLoggerScope(
        this WebApplication app
    )
    {
        var metadataProvider = app.Services.GetRequiredService<IServiceMetadataProvider>();
        var metadataCustomDimensions = metadataProvider.GetMetadata().GetCustomDimensions();

        using var globalLoggerScope = app.Logger.BeginScope(
            metadataCustomDimensions
        );

        app.Run();
    }
}
