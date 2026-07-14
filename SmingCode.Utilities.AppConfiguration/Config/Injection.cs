using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace SmingCode.Utilities.AppConfiguration.Config;

public static class Injection
{
    private static readonly string _appConfigurationEndpointConfigEntryName = "App_Config_Endpoint";
    private static readonly string _tagEnvironmentConfigEntryName = "Tag_Environment";

    public static ConfigurationManager ConnectToAppConfiguration(
        this ConfigurationManager configurationManager
    )
    {
        var appConfigurationEndpoint = configurationManager.GetValue<string>(_appConfigurationEndpointConfigEntryName)
            ?? throw new InvalidOperationException($"Could not find the configuration entry {_appConfigurationEndpointConfigEntryName}.");
        var appConfigurationLabel = configurationManager.GetValue<string>(_tagEnvironmentConfigEntryName)
            ?? throw new InvalidOperationException($"Could not find the configuration entry {_tagEnvironmentConfigEntryName}.");

        configurationManager.AddAzureAppConfiguration(azureAppConfigurationOptions =>
            azureAppConfigurationOptions
                .Connect(
                    new Uri(appConfigurationEndpoint),
                    new DefaultAzureCredential()
                )
                .Select(KeyFilter.Any, LabelFilter.Null)
                .Select(KeyFilter.Any, appConfigurationLabel)
        );

        return configurationManager;
    }
}