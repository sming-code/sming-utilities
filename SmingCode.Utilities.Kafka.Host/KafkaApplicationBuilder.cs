using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;

namespace SmingCode.Utilities.Kafka.Host;
using Config;
using Consumers;
using ServiceMetadata;

public class KafkaApplicationBuilder(
    KafkaApplicationBuilderSettings? settings
) : IHostApplicationBuilder
{
    private readonly HostApplicationBuilder _hostApplicationBuilder = new (
        settings?.ToHostApplicationBuilderSettings()
    );

    public KafkaApplicationBuilder()
        : this(args: null) { }

    public KafkaApplicationBuilder(string[]? args)
        : this(new KafkaApplicationBuilderSettings { Args = args })
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
        Services.InitializeKafkaHandling(Configuration, true);
        Services.InitializeServiceMetadata();

        return _hostApplicationBuilder.Build();
    }

    public IKafkaConsumerDefinition MapConsumer(
        string topicToMatch,
        Delegate handler
    ) => Services.MapConsumer(
        topicToMatch,
        handler
    );
}
