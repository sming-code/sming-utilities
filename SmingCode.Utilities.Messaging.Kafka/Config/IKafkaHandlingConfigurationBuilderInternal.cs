namespace SmingCode.Utilities.Messaging.Kafka.Config;

using Consumers;

internal interface IKafkaHandlingConfigurationBuilderInternal
{
    IServiceCollection Services { get; }
    IKafkaHandlingConfigurationBuilder WithGlobalPrerequisiteHandler<IHandler>()
        where IHandler : IKafkaConsumerPreInitProcessHandler;
}
