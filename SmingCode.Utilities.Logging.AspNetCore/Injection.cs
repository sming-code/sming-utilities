using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace SmingCode.Utilities.Logging.AspNetCore;

public static class Injection
{
    public static WebApplicationBuilder InitializeLogging(
        this WebApplicationBuilder builder
    )
    {
        builder.Services.AddOpenTelemetry()
            .UseAzureMonitor();

        builder.Logging.AddOpenTelemetry(options => options.IncludeScopes = true);

        return builder;
    }
}