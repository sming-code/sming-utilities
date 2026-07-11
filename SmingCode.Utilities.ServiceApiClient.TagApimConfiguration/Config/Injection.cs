using Microsoft.Extensions.DependencyInjection;

namespace SmingCode.Utilities.ServiceApiClient.TagApimConfiguration.Config;

using Microsoft.Extensions.Configuration;
using ServiceApiClient.Config;

public static class Injection
{
    public static IServiceApiClientConfigurationBuilder<TService> AddTagApimConfiguration<TService>(
        this IServiceApiClientConfigurationBuilder<TService> builder,
        IConfiguration configuration,
        string configurationEntryName
    ) where TService : class
    {
        if (builder is not ServiceApiClientConfigurationBuilder<TService> serviceApiClientConfigurationBuilder)
        {
            throw new Exception();
        }

        var subKey = configuration[$"Apis:{configurationEntryName}:ApimConfiguration:SubscriptionKey"]
            ?? throw new InvalidOperationException(
                "Must add subscription key.."
            );
        var targetVersion = configuration[$"Apis:{configurationEntryName}:ApimConfiguration:ApiVersion"]
            ?? throw new InvalidOperationException(
                "Must add api version.."
            );

        var options = new ApimConfigurationOptions(
            targetVersion,
            subKey
        );
        serviceApiClientConfigurationBuilder.AddClientSpecificSingleton(options);
        serviceApiClientConfigurationBuilder.AddApiClientSpecificMiddleware<ApimConfigurationMiddleware>();

        return builder;
    }
}
