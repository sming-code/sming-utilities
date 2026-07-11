using System.Reflection;

namespace SmingCode.Utilities.Kafka.Consumers;
using DelegateInvokers;

internal class KafkaDelegateInvoker<TKey, TValue>
{
    private readonly IDelegateInvoker<IServiceProvider, KafkaConsumerContext, KafkaEventResult> _invoker;

    internal class ParameterBuilderBuilder : DelegateParameterBuilderBuilder<IServiceProvider, KafkaConsumerContext>
    {
        public override Func<IServiceProvider, KafkaConsumerContext, TParam> BuildParameterBuilder<TParam>(
            ParameterInfo parameterInfo
        ) => parameterInfo.GetCustomAttribute<FromEventKeyAttribute>() is not null
                ? (_, context) => context.Key is not null && context.Key is TParam tParamVal
                    ? tParamVal
                    : throw new InvalidCastException("Mismatched key type in kafka message handling")
                : parameterInfo.GetCustomAttribute<FromEventValueAttribute>() is not null
                    ? (_, context) => context.Value is not null && context.Value is TParam tParamVal
                        ? tParamVal
                        : throw new InvalidCastException("Mismatched value type in kafka message handling")
                    : parameterInfo.GetCustomAttribute<FromTopicNameAttribute>() is not null
                        ? (_, context) => typeof(TParam) == typeof(string) && context.TopicConsumed is TParam tParamVal
                            ? tParamVal
                            : throw new InvalidCastException("FromTopic attribute can only be associated with string parameters.")
                        : parameterInfo.GetCustomAttribute<FromHeaderValueAttribute>() is not null
                            ? (_, context) => context.Headers.TryGetHeader<TParam>(
                                    parameterInfo.GetCustomAttribute<FromHeaderValueAttribute>()!.HeaderName,
                                    out var headerValue
                                )
                                ? headerValue
                                : throw new InvalidCastException(
                                    $"FromHeaderValue attribute either found no matched header value '{parameterInfo.Name}' or the header could not be deserialized to {typeof(TParam)}")
                            : typeof(TParam) == typeof(KafkaConsumerContext)
                                ? (_, context) => context is TParam typedContext
                                    ? typedContext
                                    : throw new Exception()
                                : (serviceProvider, _) => serviceProvider.GetService<TParam>()!;
    }

    internal KafkaDelegateInvoker(
        Delegate @delegate
    ) => _invoker = DelegateInvoker<IServiceProvider, KafkaConsumerContext, KafkaEventResult>.FromDelegate(
        @delegate,
        new ParameterBuilderBuilder()
    );

    public async Task<KafkaEventResult> Invoke(
        IServiceProvider serviceProvider,
        KafkaConsumerContext context
    ) => await _invoker.Invoke(serviceProvider, context);
}