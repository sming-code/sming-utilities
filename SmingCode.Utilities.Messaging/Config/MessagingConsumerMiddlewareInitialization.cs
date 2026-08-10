using System.Linq.Expressions;
using System.Reflection;

namespace SmingCode.Utilities.Messaging.Config;
using Consumers;
using StartupProcesses;

internal class MessagingConsumerMiddlewareInitialization : IServiceInitializer
{
    private static readonly Type _contextType = typeof(MessagingConsumerContext);
    private static readonly PropertyInfo _contextServiceProviderProperty
        = typeof(MessagingConsumerContext)
            .GetProperty(
                nameof(MessagingConsumerContext.ServiceProvider),
                BindingFlags.NonPublic | BindingFlags.Instance
            )!;
    private static readonly MethodInfo _consumeDelegateBuilder =
        typeof(MessagingConsumerMiddlewareInitialization)
            .GetMethod(
                nameof(BuildConsumerDelegate),
                BindingFlags.Static | BindingFlags.NonPublic
            )!;
    private static readonly MethodInfo _getRequiredServiceMethod =
        typeof(ServiceProviderServiceExtensions)
            .GetMethods(
                BindingFlags.Static | BindingFlags.Public
            )
            .Single(method =>
                method.Name == "GetRequiredService"
                && method.IsGenericMethod
            );

    public Delegate ServiceInitializer =>
        (
            IServiceProvider serviceProvider,
            ConsumerMiddlewareHandler middlewareHandler,
            IEnumerable<ConsumerMiddlewareDetail>? middlewareDetails
        ) =>
        {
            ConsumeDelegate nextPipelineEntryDelegate = async context => await context.MessageConsumer(context);

            if (middlewareDetails is not null)
            {
                foreach (var middleware in middlewareDetails.OrderBy(mid => mid.ProcessPosition))
                {
                    var buildApiDelegateMethod = _consumeDelegateBuilder
                        !.MakeGenericMethod(middleware.MiddlewareImplementation);

                    var delegateMethod = (ConsumeDelegate)buildApiDelegateMethod.Invoke(null, [ nextPipelineEntryDelegate, serviceProvider ])!;

                    nextPipelineEntryDelegate = delegateMethod;
                }
            }

            middlewareHandler.SetMessageConsumer(nextPipelineEntryDelegate);
        };

    private static ConsumeDelegate BuildConsumerDelegate<T>(
        ConsumeDelegate nextPipelineEntryDelegate,
        IServiceProvider serviceProvider
    )
    {
        var middlewareType = typeof(T);

        var middlewareSingletonInstance = ActivatorUtilities.CreateInstance<T>(serviceProvider, nextPipelineEntryDelegate);

        Expression[] parameterBuilderExpressions = [];
        var handleAsyncMethod = middlewareType.GetMethod("HandleAsync");
        if (handleAsyncMethod is null || handleAsyncMethod.ReturnType != typeof(Task))
        {
            throw new InvalidOperationException(
                $"Attempt to inject Consumer middleware {middlewareType.Name} failed as it has no HandleAsync method with return type Task."
            );
        }

        var handleAsyncMethodParameters = handleAsyncMethod.GetParameters();
        var contextParameters = handleAsyncMethodParameters.Where(param =>
            param.ParameterType == _contextType
            && param.Name == "context"
        ).ToArray();
        if (contextParameters.Length != 1)
        {
            throw new InvalidOperationException(
                $"Attempt to inject Consumer middleware {middlewareType.Name} failed as it's HandleAsync must have exactly one parameter of type {nameof(MessagingConsumerContext)} with name 'context'."
            );
        }

        var instanceParameter = Expression.Parameter(middlewareType, "instance");
        var contextParameter = Expression.Parameter(_contextType, "context");
        var serviceProviderProperty = Expression.Property(contextParameter, _contextServiceProviderProperty);

        foreach (var handleAsyncParameter in handleAsyncMethodParameters)
        {
            var parameterType = handleAsyncParameter.ParameterType;
            var parameterName = handleAsyncParameter.Name;

            parameterBuilderExpressions = [
                .. parameterBuilderExpressions,
                parameterType == _contextType
                    ? contextParameter
                    : Expression.Call(null, _getRequiredServiceMethod.MakeGenericMethod(parameterType), serviceProviderProperty)
            ];
        }
        var methodCall = Expression.Call(instanceParameter, handleAsyncMethod, parameterBuilderExpressions);
        var method = Expression.Lambda<Func<T, MessagingConsumerContext, Task>>(methodCall, instanceParameter, contextParameter).Compile();

        return async context => await method(middlewareSingletonInstance, context);
    }
}