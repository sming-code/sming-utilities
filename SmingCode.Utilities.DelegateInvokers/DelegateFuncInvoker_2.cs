namespace SmingCode.Utilities.DelegateInvokers;

public interface IDelegateFuncInvoker<TIn, TResult>
{
    Task<TResult> Invoke(TIn input);
}

public static class DelegateFuncInvoker<TIn1, TResult>
{
    private static readonly Type _syncResultType = typeof(TResult);
    private static readonly Type _asyncResultType = typeof(Task<>).MakeGenericType(typeof(TResult));
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

    public static IDelegateFuncInvoker<TIn1, TResult> FromDelegate(
        Delegate @delegate,
        DelegateParameterBuilderBuilder<TIn1> parameterBuilderBuilder
    )
    {
        var delegateGenericArguments = @delegate.GetType().GetGenericArguments();
        var delegateResultType = delegateGenericArguments.Last();
        if (delegateResultType != _syncResultType
            && delegateResultType != _asyncResultType)
        {
            throw new InvalidOperationException(
                $"Response type of delegate must be either {_syncResultType.Name} or {_asyncResultType.Name}"
            );
        }

        var delegateMethodParameterInfos = @delegate.Method.GetParameters();
        if (!_invokers.TryGetValue(
            delegateMethodParameterInfos.Length,
            out var invokerType
        ))
        {
            throw new InvalidOperationException(
                $"Delegate passed has too many parameters to be invoked by the DelegateFuncInvoker."
            );
        }

        var _isInvokerAsync = delegateResultType == _asyncResultType;
        Type[] invokerTypeGenericArguments = [
            typeof(TIn1),
            typeof(TResult),
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

        return (IDelegateFuncInvoker<TIn1, TResult>)Activator.CreateInstance(
            invokerGenericType,
            invokerConstructorParams
        )!;
    }

    internal class Invoker(
        Delegate @delegate,
        bool isAsyncDelegate
    ) : IDelegateFuncInvoker<TIn1, TResult>
    {
        private readonly Func<Task<TResult>> _func = isAsyncDelegate
            ? (Func<Task<TResult>>)@delegate
            : async () => await Task.FromResult(((Func<TResult>)@delegate)());

        public async Task<TResult> Invoke(TIn1 _)
            => await _func();
    }

    internal class Invoker<TParam>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam> _paramBuilder
    ) : IDelegateFuncInvoker<TIn1, TResult>
    {
        private readonly Func<TParam, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam, Task<TResult>>)@delegate
            : async (param) => await Task.FromResult(((Func<TParam, TResult>)@delegate)(param));

        public async Task<TResult> Invoke(TIn1 input)
            => await _func(_paramBuilder(input));
    }

    internal class Invoker<TParam1, TParam2>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TParam1> _paramBuilder1,
        Func<TIn1, TParam2> _paramBuilder2
    ) : IDelegateFuncInvoker<TIn1, TResult>
    {
        private readonly Func<TParam1, TParam2, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, Task<TResult>>)@delegate
            : async (param1, param2) => await Task.FromResult(((Func<TParam1, TParam2, TResult>)@delegate)(param1, param2));

        public async Task<TResult> Invoke(TIn1 input)
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
    ) : IDelegateFuncInvoker<TIn1, TResult>
    {
        private readonly Func<TParam1, TParam2, TParam3, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, Task<TResult>>)@delegate
            : async (param1, param2, param3) => await Task.FromResult(
                ((Func<TParam1, TParam2, TParam3, TResult>)@delegate)(
                    param1,
                    param2,
                    param3
                )
            );

        public async Task<TResult> Invoke(TIn1 input)
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
    ) : IDelegateFuncInvoker<TIn1, TResult>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, Task<TResult>>)@delegate
            : async (param1, param2, param3, param4) => await Task.FromResult(
                ((Func<TParam1, TParam2, TParam3, TParam4, TResult>)@delegate)(
                    param1,
                    param2,
                    param3,
                    param4
                )
            );

        public async Task<TResult> Invoke(TIn1 input)
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
    ) : IDelegateFuncInvoker<TIn1, TResult>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task<TResult>>)@delegate
            : async (param1, param2, param3, param4, param5) => await Task.FromResult(
                ((Func<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>)@delegate)(
                    param1,
                    param2,
                    param3,
                    param4,
                    param5
                )
            );

        public async Task<TResult> Invoke(TIn1 input)
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
    ) : IDelegateFuncInvoker<TIn1, TResult>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task<TResult>>)@delegate
            : async (param1, param2, param3, param4, param5, param6) => await Task.FromResult(
                ((Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>)@delegate)(
                    param1,
                    param2,
                    param3,
                    param4,
                    param5,
                    param6
                )
            );

        public async Task<TResult> Invoke(TIn1 input)
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
    ) : IDelegateFuncInvoker<TIn1, TResult>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task<TResult>>)@delegate
            : async (param1, param2, param3, param4, param5, param6, param7) => await Task.FromResult(
                ((Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>)@delegate)(
                    param1,
                    param2,
                    param3,
                    param4,
                    param5,
                    param6,
                    param7
                )
            );

        public async Task<TResult> Invoke(TIn1 input)
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
    ) : IDelegateFuncInvoker<TIn1, TResult>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, Task<TResult>>)@delegate
            : async (param1, param2, param3, param4, param5, param6, param7, param8) => await Task.FromResult(
                ((Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>)@delegate)(
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

        public async Task<TResult> Invoke(TIn1 input)
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
    ) : IDelegateFuncInvoker<TIn1, TResult>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, Task<TResult>>)@delegate
            : async (param1, param2, param3, param4, param5, param6, param7, param8, param9) => await Task.FromResult(
                ((Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>)@delegate)(
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

        public async Task<TResult> Invoke(TIn1 input)
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