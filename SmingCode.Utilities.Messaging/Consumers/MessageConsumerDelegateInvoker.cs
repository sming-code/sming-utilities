using System.Reflection;

namespace SmingCode.Utilities.Messaging.Consumers;
using DelegateInvokers;

internal class MessageConsumerDelegateInvoker<TBody>
{
    private readonly IDelegateActionInvoker<IServiceProvider, MessagingConsumerContext> _invoker;

    internal class ParameterBuilderBuilder : DelegateParameterBuilderBuilder<IServiceProvider, MessagingConsumerContext>
    {
        public override Func<IServiceProvider, MessagingConsumerContext, TParam> BuildParameterBuilder<TParam>(
            ParameterInfo parameterInfo
        ) => parameterInfo.GetCustomAttribute<FromMessageBodyAttribute>() is not null
                ? (_, context) => context.Value is not null && context.Value is TParam tParamVal
                    ? tParamVal
                    : throw new InvalidCastException("Mismatched value type in kafka message handling")
                : parameterInfo.GetCustomAttribute<FromTopicNameAttribute>() is not null
                    ? (_, context) => typeof(TParam) == typeof(string) && context.TopicConsumed is TParam tParamVal
                        ? tParamVal
                        : throw new InvalidCastException("FromTopic attribute can only be associated with string parameters.")
                    : parameterInfo.GetCustomAttribute<FromMetadataAttribute>() is not null
                        ? (_, context) => context.MetadataCollection.TryGetMetadata<TParam>(
                                parameterInfo.GetCustomAttribute<FromMetadataAttribute>()!.MetadataPropertyName,
                                out var metadataEntryValue
                            )
                            ? metadataEntryValue
                            : throw new InvalidCastException(
                                $"FromMetadataValue attribute either found no matched metadata entry for '{parameterInfo.Name}' or the metadata entry could not be deserialized to {typeof(TParam)}")
                        : typeof(TParam) == typeof(MessagingConsumerContext)
                            ? (_, context) => context is TParam typedContext
                                ? typedContext
                                : throw new Exception()
                            : (serviceProvider, _) => serviceProvider.GetService<TParam>()!;
    }

    internal MessageConsumerDelegateInvoker(
        Delegate @delegate
    ) => _invoker = DelegateActionInvoker<IServiceProvider, MessagingConsumerContext>.FromDelegate(
        @delegate,
        new ParameterBuilderBuilder()
    );

    public async Task Invoke(
        IServiceProvider serviceProvider,
        MessagingConsumerContext context
    ) => await _invoker.Invoke(serviceProvider, context);
}