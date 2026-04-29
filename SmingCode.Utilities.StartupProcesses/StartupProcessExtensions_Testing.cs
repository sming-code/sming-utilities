using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SmingCode.Utilities.StartupProcesses;

public static class StartupProcessExtensions_Testing
{
    public static async Task<IServiceProvider> RunUserDefinedStartupProcesses(
        this IServiceProvider singletonServiceProvider,
        Action<IStartupProcessDependency>? dependencyManager = null
    )
    {
        using var scope = singletonServiceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var serviceInitializers = serviceProvider.GetService<IEnumerable<IServiceInitializer>>()
            ?.ToArray();

        if (serviceInitializers is null || serviceInitializers.Length == 0)
        {
            return serviceProvider;
        }

        var startProcessInvokersProvider = new StartupProcessDelegateInvokerProvider();
        if (dependencyManager is not null)
        {
            dependencyManager(startProcessInvokersProvider);
        }
        var delegateInvokers = startProcessInvokersProvider.GetStartupProcessDelegateInvokers();

        foreach (var serviceInitializer in serviceInitializers)
        {
            var success = await TryRunningServiceInitializer(
                serviceInitializer,
                delegateInvokers,
                serviceProvider
            );

            if (!success)
            {
                throw new Exception("Unable to run all service initializers. Some startup processes may require additional startup processors to be loaded.");
            }
        }

        return serviceProvider;
    }

    private static async Task<bool> TryRunningServiceInitializer(
        IServiceInitializer serviceInitializer,
        IStartupProcessDelegateInvoker[] delegateInvokers,
        IServiceProvider serviceProvider
    )
    {
        foreach (var delegateInvoker in delegateInvokers)
        {
            if (await delegateInvoker.TryInvoke(
                null!,
                serviceProvider,
                serviceInitializer.ServiceInitializer
            ))
            {
                return true;
            }
        }

        return false;
    }
}
