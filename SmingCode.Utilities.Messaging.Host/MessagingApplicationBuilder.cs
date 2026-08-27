using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;

namespace SmingCode.Utilities.Messaging.Host;
using Config;
using Consumers;
using ServiceMetadata.Config;

public class MessagingApplicationBuilder(
    MessagingApplicationBuilderSettings? settings
) : IHostApplicationBuilder
{
    private readonly HostApplicationBuilder _hostApplicationBuilder = new (
        settings?.ToHostApplicationBuilderSettings()
    );
    private Action<IMessageHandlingConfigurationBuilder>? _messageHandlingConfigurationBuilder;

    public MessagingApplicationBuilder()
        : this(args: null) { }

    public MessagingApplicationBuilder(string[]? args)
        : this(new MessagingApplicationBuilderSettings { Args = args })
    { }

    public IHostEnvironment Environment => _hostApplicationBuilder.Environment;
    public ConfigurationManager Configuration => _hostApplicationBuilder.Configuration;
    IConfigurationManager IHostApplicationBuilder.Configuration => Configuration;
    public IServiceCollection Services => _hostApplicationBuilder.Services;
    public ILoggingBuilder Logging => _hostApplicationBuilder.Logging;
    public IMetricsBuilder Metrics => _hostApplicationBuilder.Metrics;

    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null
    ) where TContainerBuilder : notnull
        => _hostApplicationBuilder.ConfigureContainer(factory, configure);

    public IDictionary<object, object> Properties => ((IHostApplicationBuilder)_hostApplicationBuilder).Properties;
    IDictionary<object, object> IHostApplicationBuilder.Properties => throw new NotImplementedException();

    public IHost Build()
    {
        if (_messageHandlingConfigurationBuilder is null)
        {
            throw new InvalidOperationException(
                "You must configure message handling before building the messaging application"
            );
        }
        
        Services.InitializeMessageHandling(
            Configuration,
            _messageHandlingConfigurationBuilder
        );
        Services.InitializeServiceMetadata();

        return _hostApplicationBuilder.Build();
    }

    public MessagingApplicationBuilder ConfigureMessageHandling(
        Action<IMessageHandlingConfigurationBuilder> messageHandlingConfigurationBuilder
    )
    {
        _messageHandlingConfigurationBuilder = messageHandlingConfigurationBuilder;

        return this;
    }

    public IMessagingConsumerDefinition MapConsumer(
        string topicToMatch,
        Delegate handler
    ) => Services.MapConsumer(
        topicToMatch,
        handler
    );
}
