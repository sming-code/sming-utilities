namespace SmingCode.Utilities.Messaging.Config;

internal class MessageHandlingConfigurationBuilder(
    IServiceCollection services
) : IMessageHandlingConfigurationBuilder
{
    internal IServiceCollection Services { get; } = services;
}