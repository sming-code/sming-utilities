using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SmingCode.Utilities.AzureIdentity.Config;

public static class Injection
{
    private static DefaultAzureCredentialOptions _defaultAzureCredentialOptions = new()
    {
        ExcludeEnvironmentCredential = true,
        ExcludeWorkloadIdentityCredential = true
    };

    public static IServiceCollection InitializeAzureIdentity(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        out TokenCredential tokenCredential
    )
    {
        Func<TokenCredential> tokenCredentialFactory = hostEnvironment.IsDevelopment()
            ? () => new AzureCliCredential()
            : () => new DefaultAzureCredential(_defaultAzureCredentialOptions);
        
        var providerHelper = new AzureIdentityProviderHelper(tokenCredentialFactory);
        services.AddSingleton(providerHelper);

        tokenCredential = tokenCredentialFactory();
        return services;
    }
}
