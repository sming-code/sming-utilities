namespace SmingCode.Utilities.DelegateInvokers;

public interface IDelegateActionInvoker<TIn1>
{
    Task Invoke(TIn1 input);
}

public static class DelegateActionInvoker<TIn1>
{
    private static readonly Type _asyncResultType = typeof(Task);
    private static readonly Type _parameterBuilderBuilderType = typeof(DelegateParameterBuilderBuilder<TIn1>);
    private static readonly string _buildParameterBuilderMethodName = nameof(DelegateParameterBuilderBuilder<>.BuildParameterBuilder);
    private static readonly Dictionary<int, Type> _invokers = new()
    {
        { 0, typeof(Invoker) },
        { 1, typeof(Invoker<>) },
        { 2, typeof(Invoker<,>) },
        { 3, typeof(Invoker<,,>) },
        { 4, typeof(Invoker<,,,>) },
        { 5, typeof(Invoker<,,,,>) },
        { 6, typeof(Invoker<,,,,,>) },
        { 7, typeof(Invoker<,,,,,,>) },
        { 8, typeof(Invoker<,,,,,,,>) },
        { 9, typeof(Invoker<,,,,,,,,>) }
    };

    public static IDelegateActionInvoker<TIn1> FromDelegate(
        Delegate @delegate,
        DelegateParameterBuilderBuilder<TIn1> parameterBuilderBuilder
    )
    {
        var delegateGenericArguments = @delegate.GetType().GetGenericArguments();
        var delegateResultType = delegateGenericArguments.LastOrDefault();
        if (delegateResultType != null
            && delegateResultType != _asyncResultType)
        {
            throw new InvalidOperationException(
                $"There should be no response type from an action delegate."
            );
        }

        var delegateMethodParameterInfos = @delegate.Method.GetParameters();
        if (!_invokers.TryGetValue(
            delegateMethodParameterInfos.Length,
            out var invokerType
        ))
        {
            throw new InvalidOperationException(
                $"Delegate passed has too many parameters to be invoked by the DelegateActionInvoker."
            );
        }

        var _isInvokerAsync = delegateResultType == _asyncResultType;
        Type[] invokerTypeGenericArguments = [
            typeof(TIn1),
            ..delegateMethodParameterInfos.Select(paramInfo => paramInfo.ParameterType)
        ];
        var invokerGenericType = invokerType.MakeGenericType(invokerTypeGenericArguments);
        object[] invokerConstructorParams = [
            @delegate,
            _isInvokerAsync,
            .. delegateMethodParameterInfos
                .Select(parameter =>
                {
                    var parameterBuilderBuilderMethodCall = _parameterBuilderBuilderType
                        .GetMethod(_buildParameterBuilderMethodName)!
                        .MakeGenericMethod(parameter.ParameterType);

                    return parameterBuilderBuilderMethodCall.Invoke(parameterBuilderBuilder, [ parameter ])!;
                })
        ];

        return (IDelegateActionInvoker<TIn1>)Activator.CreateInstance(
            invokerGenericType,
            invokerConstructorParams
        )!;
    }

    internal class Invoker(
        Delegate @delegate,
        bool isAsyncDelegate
    ) : IDelegateActionInvoker<TIn1>
    {
        private readonly Func<Task> _func = isAsyncDelegate
            ? (Func<Task>)@delegate
            : async () => await Task.Run(() => ((Action)@delegate)());

        public async Task Invoke(TIn1 _)
            => await _func();
    }

    internal class Invoker<TParam>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam> _paramBuilder
    ) : IDelegateActionInvoker<TIn1>
    {
        private readonly Func<TParam, Task> _func = isAsyncDelegate
            ? (Func<TParam, Task>)@delegate
            : async (param) => await Task.Run(() => ((Action<TParam>)@delegate)(param));

        public async Task Invoke(TIn1 input)
            => await _func(_paramBuilder(input));
    }

    internal class Invoker<TParam1, TParam2>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam1> _paramBuilder1,
        Func<TIn1, TParam2> _paramBuilder2
    ) : IDelegateActionInvoker<TIn1>
    {
        private readonly Func<TParam1, TParam2, Task> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, Task>)@delegate
            : async (param1, param2) => await Task.Run(() => ((Action<TParam1, TParam2>)@delegate)(param1, param2));

        public async Task Invoke(TIn1 input)
            => await _func(
                _paramBuilder1(input),
                _paramBuilder2(input)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam1> _paramBuilder1,
        Func<TIn1, TParam2> _paramBuilder2,
        Func<TIn1, TParam3> _paramBuilder3
    ) : IDelegateActionInvoker<TIn1>
    {
        private readonly Func<TParam1, TParam2, TParam3, Task> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, Task>)@delegate
            : async (param1, param2, param3) => await Task.Run(() => 
                ((Action<TParam1, TParam2, TParam3>)@delegate)(
                    param1,
                    param2,
                    param3
                )
            );

        public async Task Invoke(TIn1 input)
            => await _func(
                _paramBuilder1(input),
                _paramBuilder2(input),
                _paramBuilder3(input)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam1> _paramBuilder1,
        Func<TIn1, TParam2> _paramBuilder2,
        Func<TIn1, TParam3> _paramBuilder3,
        Func<TIn1, TParam4> _paramBuilder4
    ) : IDelegateActionInvoker<TIn1>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, Task> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, Task>)@delegate
            : async (param1, param2, param3, param4) => await Task.Run(() => 
                ((Action<TParam1, TParam2, TParam3, TParam4>)@delegate)(
                    param1,
                    param2,
                    param3,
                    param4
                )
            );

        public async Task Invoke(TIn1 input)
            => await _func(
                _paramBuilder1(input),
                _paramBuilder2(input),
                _paramBuilder3(input),
                _paramBuilder4(input)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4, TParam5>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam1> _paramBuilder1,
        Func<TIn1, TParam2> _paramBuilder2,
        Func<TIn1, TParam3> _paramBuilder3,
        Func<TIn1, TParam4> _paramBuilder4,
        Func<TIn1, TParam5> _paramBuilder5
    ) : IDelegateActionInvoker<TIn1>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task>)@delegate
            : async (param1, param2, param3, param4, param5) => await Task.Run(() => 
                ((Action<TParam1, TParam2, TParam3, TParam4, TParam5>)@delegate)(
                    param1,
                    param2,
                    param3,
                    param4,
                    param5
                )
            );

        public async Task Invoke(TIn1 input)
            => await _func(
                _paramBuilder1(input),
                _paramBuilder2(input),
                _paramBuilder3(input),
                _paramBuilder4(input),
                _paramBuilder5(input)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam1> _paramBuilder1,
        Func<TIn1, TParam2> _paramBuilder2,
        Func<TIn1, TParam3> _paramBuilder3,
        Func<TIn1, TParam4> _paramBuilder4,
        Func<TIn1, TParam5> _paramBuilder5,
        Func<TIn1, TParam6> _paramBuilder6
    ) : IDelegateActionInvoker<TIn1>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task>)@delegate
            : async (param1, param2, param3, param4, param5, param6) => await Task.Run(() => 
                ((Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>)@delegate)(
                    param1,
                    param2,
                    param3,
                    param4,
                    param5,
                    param6
                )
            );

        public async Task Invoke(TIn1 input)
            => await _func(
                _paramBuilder1(input),
                _paramBuilder2(input),
                _paramBuilder3(input),
                _paramBuilder4(input),
                _paramBuilder5(input),
                _paramBuilder6(input)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam1> _paramBuilder1,
        Func<TIn1, TParam2> _paramBuilder2,
        Func<TIn1, TParam3> _paramBuilder3,
        Func<TIn1, TParam4> _paramBuilder4,
        Func<TIn1, TParam5> _paramBuilder5,
        Func<TIn1, TParam6> _paramBuilder6,
        Func<TIn1, TParam7> _paramBuilder7
    ) : IDelegateActionInvoker<TIn1>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task>)@delegate
            : async (param1, param2, param3, param4, param5, param6, param7) => await Task.Run(() => 
                ((Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>)@delegate)(
                    param1,
                    param2,
                    param3,
                    param4,
                    param5,
                    param6,
                    param7
                )
            );

        public async Task Invoke(TIn1 input)
            => await _func(
                _paramBuilder1(input),
                _paramBuilder2(input),
                _paramBuilder3(input),
                _paramBuilder4(input),
                _paramBuilder5(input),
                _paramBuilder6(input),
                _paramBuilder7(input)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam1> _paramBuilder1,
        Func<TIn1, TParam2> _paramBuilder2,
        Func<TIn1, TParam3> _paramBuilder3,
        Func<TIn1, TParam4> _paramBuilder4,
        Func<TIn1, TParam5> _paramBuilder5,
        Func<TIn1, TParam6> _paramBuilder6,
        Func<TIn1, TParam7> _paramBuilder7,
        Func<TIn1, TParam8> _paramBuilder8
    ) : IDelegateActionInvoker<TIn1>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, Task> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, Task>)@delegate
            : async (param1, param2, param3, param4, param5, param6, param7, param8) => await Task.Run(() => 
                ((Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>)@delegate)(
                    param1,
                    param2,
                    param3,
                    param4,
                    param5,
                    param6,
                    param7,
                    param8
                )
            );

        public async Task Invoke(TIn1 input)
            => await _func(
                _paramBuilder1(input),
                _paramBuilder2(input),
                _paramBuilder3(input),
                _paramBuilder4(input),
                _paramBuilder5(input),
                _paramBuilder6(input),
                _paramBuilder7(input),
                _paramBuilder8(input)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam1> _paramBuilder1,
        Func<TIn1, TParam2> _paramBuilder2,
        Func<TIn1, TParam3> _paramBuilder3,
        Func<TIn1, TParam4> _paramBuilder4,
        Func<TIn1, TParam5> _paramBuilder5,
        Func<TIn1, TParam6> _paramBuilder6,
        Func<TIn1, TParam7> _paramBuilder7,
        Func<TIn1, TParam8> _paramBuilder8,
        Func<TIn1, TParam9> _paramBuilder9
    ) : IDelegateActionInvoker<TIn1>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, Task> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, Task>)@delegate
            : async (param1, param2, param3, param4, param5, param6, param7, param8, param9) => await Task.Run(() => 
                ((Action<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>)@delegate)(
                    param1,
                    param2,
                    param3,
                    param4,
                    param5,
                    param6,
                    param7,
                    param8,
                    param9
                )
            );

        public async Task Invoke(TIn1 input)
            => await _func(
                _paramBuilder1(input),
                _paramBuilder2(input),
                _paramBuilder3(input),
                _paramBuilder4(input),
                _paramBuilder5(input),
                _paramBuilder6(input),
                _paramBuilder7(input),
                _paramBuilder8(input),
                _paramBuilder9(input)
            );
    }
}