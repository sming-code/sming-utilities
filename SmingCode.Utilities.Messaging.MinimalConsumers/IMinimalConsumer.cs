using Microsoft.Extensions.DependencyInjection;

namespace SmingCode.Utilities.Messaging.MinimalConsumers;

public interface IMinimalConsumer
{
    void Consume(IServiceCollection services);
}
