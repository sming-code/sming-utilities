using System.Linq.Expressions;
using System.Reflection;

namespace SmingCode.Utilities.Kafka.Config;
using Consumers;
using StartupProcesses;

internal class KafkaConsumerMiddlewareInitialization : IServiceInitializer
{
    private static readonly Type _contextType = typeof(KafkaConsumerContext);
    private static readonly PropertyInfo _contextServiceProviderProperty
        = typeof(KafkaConsumerContext)
            .GetProperty(
                nameof(KafkaConsumerContext.ServiceProvider),
                BindingFlags.NonPublic | BindingFlags.Instance
            )!;
    private static readonly MethodInfo _KafkaConsumeDelegateBuilder =
        typeof(KafkaConsumerMiddlewareInitialization)
            .GetMethod(
                "BuildKafkaConsumerDelegate",
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
                foreach (var middleware in middlewareDetails.Reverse())
                {
                    var buildApiDelegateMethod = _KafkaConsumeDelegateBuilder
                        !.MakeGenericMethod(middleware.MiddlewareImplementation);

                    var delegateMethod = (ConsumeDelegate)buildApiDelegateMethod.Invoke(null, [ nextPipelineEntryDelegate, serviceProvider ])!;

                    nextPipelineEntryDelegate = delegateMethod;
                }
            }

            middlewareHandler.SetMessageConsumer(nextPipelineEntryDelegate);
        };

    private static ConsumeDelegate BuildKafkaConsumerDelegate<T>(
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
                $"Attempt to inject KafkaConsumer middleware {middlewareType.Name} failed as it has no HandleAsync method with return type Task<KafkaEventResult>"
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
                $"Attempt to inject KafkaConsumer middleware {middlewareType.Name} failed as it's HandleAsync must have exactly one parameter of type {nameof(KafkaConsumerContext)} with name 'context'."
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
        var method = Expression.Lambda<Func<T, KafkaConsumerContext, Task<KafkaEventResult>>>(methodCall, instanceParameter, contextParameter).Compile();

        return async context => await method(middlewareSingletonInstance, context);
    }
}