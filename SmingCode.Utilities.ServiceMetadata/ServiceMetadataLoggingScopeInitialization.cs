using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ServiceMetadata;
using StartupProcesses;

internal class ServiceMetadataLoggingScopeInitialization : IServiceInitializer
{
    public Delegate ServiceInitializer =>
        (
            ILoggerProvider loggerProvider,
            IServiceMetadataProvider serviceMetadataProvider
        ) =>
        {
            if (loggerProvider is ISupportExternalScope scopedProvider)
            {
                var scopes = new LoggerExternalScopeProvider();
                scopes.Push(
                    serviceMetadataProvider
                        .GetMetadata()
                        .GetCustomDimensions()
                );
                scopedProvider.SetScopeProvider(scopes);
            }
        };
}