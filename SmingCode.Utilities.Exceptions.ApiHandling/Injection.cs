using Microsoft.Extensions.DependencyInjection;

namespace SmingCode.Utilities.Exceptions.ApiHandling;
using StartupProcesses;

public static class DependencyInjection
{
    public static IServiceCollection AddExceptionHandling(
        this IServiceCollection services
    )
    {
        services.AddExceptionHandler<ExceptionHandler>();
        services.AddScoped<IServiceInitializer, ExceptionHandlingInitialization>();

        return services;
    }
}

