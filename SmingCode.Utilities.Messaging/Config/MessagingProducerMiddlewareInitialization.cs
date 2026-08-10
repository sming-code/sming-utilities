using System.Linq.Expressions;
using System.Reflection;

namespace SmingCode.Utilities.Messaging.Config;
using Producers;
using StartupProcesses;

internal class MessagingProducerMiddlewareInitialization : IServiceInitializer
{
    private static readonly Type _contextType = typeof(MessagingProducerContext);
    private static readonly PropertyInfo _contextServiceProviderProperty
        = typeof(MessagingProducerContext)
            .GetProperty(
                nameof(MessagingProducerContext.ServiceProvider),
                BindingFlags.NonPublic | BindingFlags.Instance
            )!;
    private static readonly MethodInfo _producerDelegateBuilder =
        typeof(MessagingProducerMiddlewareInitialization)
            .GetMethod(
                nameof(BuildProducerDelegate),
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
            ProducerMiddlewareHandler middlewareHandler,
            IEnumerable<ProducerMiddlewareDetail>? middlewareDetails
        ) =>
        {
            ProducerDelegate nextPipelineEntryDelegate = async context => await context.MessageProducer(context);

            if (middlewareDetails is not null)
            {
                foreach (var middleware in middlewareDetails.OrderByDescending(mid => mid.ProcessPosition))
                {
                    var buildApiDelegateMethod = _producerDelegateBuilder
                        !.MakeGenericMethod(middleware.MiddlewareImplementation);

                    var delegateMethod = (ProducerDelegate)buildApiDelegateMethod.Invoke(null, [ nextPipelineEntryDelegate, serviceProvider ])!;

                    nextPipelineEntryDelegate = delegateMethod;
                }
            }

            middlewareHandler.SetMessageProducer(nextPipelineEntryDelegate);
        };

    private static ProducerDelegate BuildProducerDelegate<T>(
        ProducerDelegate nextPipelineEntryDelegate,
        IServiceProvider serviceProvider
    )
    {
        var middlewareType = typeof(T);

        var middlewareSingletonInstance = ActivatorUtilities.CreateInstance<T>(serviceProvider, nextPipelineEntryDelegate);

        Expression[] parameterBuilderExpressions = [];
        var handleAsyncMethod = middlewareType.GetMethod("HandleAsync");
        if (handleAsyncMethod is null || handleAsyncMethod.ReturnType != typeof(Task<bool>))
        {
            throw new InvalidOperationException(
                $"Attempt to inject Producer middleware {middlewareType.Name} failed as it has no HandleAsync method with return type Task<bool>"
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
                $"Attempt to inject Producer middleware {middlewareType.Name} failed as it's HandleAsync must have exactly one parameter of type {nameof(MessagingProducerContext)} with name 'context'."
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
        var method = Expression.Lambda<Func<T, MessagingProducerContext, Task<bool>>>(methodCall, instanceParameter, contextParameter).Compile();

        return async context => await method(middlewareSingletonInstance, context);
    }
}