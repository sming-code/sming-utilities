using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using OpenTelemetry;
using System.Diagnostics;

namespace SmingCode.Utilities.Logging.AspNetCore;
using ServiceMetadata;

public static class Injection
{
    public static WebApplicationBuilder InitializeLogging(
        this WebApplicationBuilder builder
    )
    {
        builder.Services.AddOpenTelemetry()
            .UseAzureMonitor()
            .WithTracing(builder => builder.AddProcessor<ServiceMetadataOpenTelemetryActivityEnrichingProcessor>());

        builder.Logging.AddOpenTelemetry(options => options.IncludeScopes = true);

        return builder;
    }
}

internal class ServiceMetadataOpenTelemetryActivityEnrichingProcessor(
    IServiceMetadataProvider _serviceMetadataProvider
) : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        foreach (var serviceMetadataDimension in _serviceMetadataProvider.GetMetadata().GetCustomDimensions())
        {
            activity.SetTag(serviceMetadataDimension.Key, serviceMetadataDimension.Value);
        }
    }
}