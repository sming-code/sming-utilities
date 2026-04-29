using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmingCode.Utilities.StartupProcesses;

namespace SmingCode.Utilities.ServiceApiClient.Config;

public static class Configuration
{
    private static bool _serviceInitializerInjected = false;
    private const int DEFAULT_TIMEOUT_SECONDS = 60;

    public static IServiceCollection AddApiClient<TInterface, TService>(
        this IServiceCollection services,
        string targetServiceDisplayName,
        string targetServiceName
    ) where TInterface : class
      where TService : class, TInterface
      => AddApiClient<TInterface, TService>(
        services,
        targetServiceDisplayName,
        targetServiceName,
        _ => {}
      );

    public static IServiceCollection AddApiClient<TInterface, TService>(
        this IServiceCollection services,
        string targetServiceDisplayName,
        string targetServiceName,
        Action<HttpClient> clientConfiguration
    ) where TInterface : class
      where TService : class, TInterface
    {
        ApiClientConfiguration<TService> apiClientConfiguration = new(
            targetServiceDisplayName,
            targetServiceName
        );
        services.AddSingleton(apiClientConfiguration);
        services.TryAddSingleton<MiddlewareHandler>();
        if (!_serviceInitializerInjected)
        {
            services.AddScoped<IServiceInitializer, ServiceApiClientInitialization>();
            _serviceInitializerInjected = true;
        }

        services.AddHttpClient<IServiceApiClient<TService>, ApiClient<TService>>(config =>
        {
            config.BaseAddress = new Uri($"http://{targetServiceName}");
            config.Timeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS);
            clientConfiguration(config);
        });
        services.AddScoped<TInterface, TService>();
        services.TryAddScoped(typeof(ApiClientMessageSender<,>));

        return services;
    }

    public static IServiceCollection AddApiClientMiddleware<TImplementation>(
        this IServiceCollection services
    )
    {
        services.AddSingleton(new MiddlewareDetail(typeof(TImplementation)));

        return services;
    }
}
