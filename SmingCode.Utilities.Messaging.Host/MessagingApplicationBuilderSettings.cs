using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SmingCode.Utilities.Messaging.Host;

public class MessagingApplicationBuilderSettings
{
    public bool DisableDefaults { get; set; }
    public string[]? Args { get; set; }
    public ConfigurationManager? Configuration { get; set; }
    public string? EnvironmentName { get; set; }
    public string? ApplicationName { get; set; }
    public string? ContentRootPath { get; set; }

    internal HostApplicationBuilderSettings ToHostApplicationBuilderSettings()
        => new()
        {
            DisableDefaults = DisableDefaults,
            Args = Args,
            Configuration = Configuration,
            EnvironmentName = EnvironmentName,
            ApplicationName = ApplicationName,
            ContentRootPath = ContentRootPath
        };
}
