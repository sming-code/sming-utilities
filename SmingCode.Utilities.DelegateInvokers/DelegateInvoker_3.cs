using System.Reflection;

namespace SmingCode.Utilities.DelegateInvokers;

public interface IDelegateInvoker<TIn1, TIn2, TResult>
{
    Task<TResult> Invoke(TIn1 input1, TIn2 input2);
}

public abstract class DelegateParameterBuilderBuilder<TIn1, TIn2>
{
    public abstract Func<TIn1, TIn2, TParam> BuildParameterBuilder<TParam>(ParameterInfo parameterInfo);
}

public static class DelegateInvoker<TIn1, TIn2, TResult>
{
    private static readonly Type _syncResultType = typeof(TResult);
    private static readonly Type _asyncResultType = typeof(Task<>).MakeGenericType(typeof(TResult));
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

    public static IDelegateInvoker<TIn1, TIn2, TResult> FromDelegate(
        Delegate @delegate,
        DelegateParameterBuilderBuilder<TIn1, TIn2> parameterBuilderBuilder
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
                $"Delegate passed has too many parameters to be invoked by the DelegateInvoker."
            );
        }

        var _isInvokerAsync = delegateResultType == _asyncResultType;
        Type[] invokerTypeGenericArguments = [
            typeof(TIn1),
            typeof(TIn2),
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
                    var parameterBuilderBuilderMethodCall = typeof(DelegateParameterBuilderBuilder<TIn1, TIn2>)
                        .GetMethod(nameof(DelegateParameterBuilderBuilder<TIn1, TIn2>.BuildParameterBuilder))!
                        .MakeGenericMethod(parameter.ParameterType);

                    return parameterBuilderBuilderMethodCall.Invoke(parameterBuilderBuilder, [ parameter ])!;
                })
        ];

        return (IDelegateInvoker<TIn1, TIn2, TResult>)Activator.CreateInstance(
            invokerGenericType,
            invokerConstructorParams
        )!;
    }

    internal class Invoker(
        Delegate @delegate,
        bool isAsyncDelegate
    ) : IDelegateInvoker<TIn1, TIn2, TResult>
    {
        private readonly Func<Task<TResult>> _func = isAsyncDelegate
            ? (Func<Task<TResult>>)@delegate
            : async () => await Task.FromResult(((Func<TResult>)@delegate)());

        public async Task<TResult> Invoke(TIn1 _, TIn2 __)
            => await _func();
    }

    internal class Invoker<TParam>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TIn2, TParam> _paramBuilder
    ) : IDelegateInvoker<TIn1, TIn2, TResult>
    {
        private readonly Func<TParam, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam, Task<TResult>>)@delegate
            : async (param) => await Task.FromResult(((Func<TParam, TResult>)@delegate)(param));

        public async Task<TResult> Invoke(TIn1 input1, TIn2 input2)
            => await _func(_paramBuilder(input1, input2));
    }

    internal class Invoker<TParam1, TParam2>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TIn2, TParam1> _paramBuilder1,
        Func<TIn1, TIn2, TParam2> _paramBuilder2
    ) : IDelegateInvoker<TIn1, TIn2, TResult>
    {
        private readonly Func<TParam1, TParam2, Task<TResult>> _func = isAsyncDelegate
            ? (Func<TParam1, TParam2, Task<TResult>>)@delegate
            : async (param1, param2) => await Task.FromResult(((Func<TParam1, TParam2, TResult>)@delegate)(param1, param2));

        public async Task<TResult> Invoke(TIn1 input1, TIn2 input2)
            => await _func(
                _paramBuilder1(input1, input2),
                _paramBuilder2(input1, input2)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TIn2, TParam1> _paramBuilder1,
        Func<TIn1, TIn2, TParam2> _paramBuilder2,
        Func<TIn1, TIn2, TParam3> _paramBuilder3
    ) : IDelegateInvoker<TIn1, TIn2, TResult>
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

        public async Task<TResult> Invoke(TIn1 input1, TIn2 input2)
            => await _func(
                _paramBuilder1(input1, input2),
                _paramBuilder2(input1, input2),
                _paramBuilder3(input1, input2)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TIn2, TParam1> _paramBuilder1,
        Func<TIn1, TIn2, TParam2> _paramBuilder2,
        Func<TIn1, TIn2, TParam3> _paramBuilder3,
        Func<TIn1, TIn2, TParam4> _paramBuilder4
    ) : IDelegateInvoker<TIn1, TIn2, TResult>
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

        public async Task<TResult> Invoke(TIn1 input1, TIn2 input2)
            => await _func(
                _paramBuilder1(input1, input2),
                _paramBuilder2(input1, input2),
                _paramBuilder3(input1, input2),
                _paramBuilder4(input1, input2)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4, TParam5>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TIn2, TParam1> _paramBuilder1,
        Func<TIn1, TIn2, TParam2> _paramBuilder2,
        Func<TIn1, TIn2, TParam3> _paramBuilder3,
        Func<TIn1, TIn2, TParam4> _paramBuilder4,
        Func<TIn1, TIn2, TParam5> _paramBuilder5
    ) : IDelegateInvoker<TIn1, TIn2, TResult>
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

        public async Task<TResult> Invoke(TIn1 input1, TIn2 input2)
            => await _func(
                _paramBuilder1(input1, input2),
                _paramBuilder2(input1, input2),
                _paramBuilder3(input1, input2),
                _paramBuilder4(input1, input2),
                _paramBuilder5(input1, input2)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TIn2, TParam1> _paramBuilder1,
        Func<TIn1, TIn2, TParam2> _paramBuilder2,
        Func<TIn1, TIn2, TParam3> _paramBuilder3,
        Func<TIn1, TIn2, TParam4> _paramBuilder4,
        Func<TIn1, TIn2, TParam5> _paramBuilder5,
        Func<TIn1, TIn2, TParam6> _paramBuilder6
    ) : IDelegateInvoker<TIn1, TIn2, TResult>
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

        public async Task<TResult> Invoke(TIn1 input1, TIn2 input2)
            => await _func(
                _paramBuilder1(input1, input2),
                _paramBuilder2(input1, input2),
                _paramBuilder3(input1, input2),
                _paramBuilder4(input1, input2),
                _paramBuilder5(input1, input2),
                _paramBuilder6(input1, input2)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TIn2, TParam1> _paramBuilder1,
        Func<TIn1, TIn2, TParam2> _paramBuilder2,
        Func<TIn1, TIn2, TParam3> _paramBuilder3,
        Func<TIn1, TIn2, TParam4> _paramBuilder4,
        Func<TIn1, TIn2, TParam5> _paramBuilder5,
        Func<TIn1, TIn2, TParam6> _paramBuilder6,
        Func<TIn1, TIn2, TParam7> _paramBuilder7
    ) : IDelegateInvoker<TIn1, TIn2, TResult>
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

        public async Task<TResult> Invoke(TIn1 input1, TIn2 input2)
            => await _func(
                _paramBuilder1(input1, input2),
                _paramBuilder2(input1, input2),
                _paramBuilder3(input1, input2),
                _paramBuilder4(input1, input2),
                _paramBuilder5(input1, input2),
                _paramBuilder6(input1, input2),
                _paramBuilder7(input1, input2)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TIn2, TParam1> _paramBuilder1,
        Func<TIn1, TIn2, TParam2> _paramBuilder2,
        Func<TIn1, TIn2, TParam3> _paramBuilder3,
        Func<TIn1, TIn2, TParam4> _paramBuilder4,
        Func<TIn1, TIn2, TParam5> _paramBuilder5,
        Func<TIn1, TIn2, TParam6> _paramBuilder6,
        Func<TIn1, TIn2, TParam7> _paramBuilder7,
        Func<TIn1, TIn2, TParam8> _paramBuilder8
    ) : IDelegateInvoker<TIn1, TIn2, TResult>
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

        public async Task<TResult> Invoke(TIn1 input1, TIn2 input2)
            => await _func(
                _paramBuilder1(input1, input2),
                _paramBuilder2(input1, input2),
                _paramBuilder3(input1, input2),
                _paramBuilder4(input1, input2),
                _paramBuilder5(input1, input2),
                _paramBuilder6(input1, input2),
                _paramBuilder7(input1, input2),
                _paramBuilder8(input1, input2)
            );
    }

    internal class Invoker<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
        Delegate @delegate,
        bool isAsyncDelegate,
        Func<TIn1, TIn2, TParam1> _paramBuilder1,
        Func<TIn1, TIn2, TParam2> _paramBuilder2,
        Func<TIn1, TIn2, TParam3> _paramBuilder3,
        Func<TIn1, TIn2, TParam4> _paramBuilder4,
        Func<TIn1, TIn2, TParam5> _paramBuilder5,
        Func<TIn1, TIn2, TParam6> _paramBuilder6,
        Func<TIn1, TIn2, TParam7> _paramBuilder7,
        Func<TIn1, TIn2, TParam8> _paramBuilder8,
        Func<TIn1, TIn2, TParam9> _paramBuilder9
    ) : IDelegateInvoker<TIn1, TIn2, TResult>
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

        public async Task<TResult> Invoke(TIn1 input1, TIn2 input2)
            => await _func(
                _paramBuilder1(input1, input2),
                _paramBuilder2(input1, input2),
                _paramBuilder3(input1, input2),
                _paramBuilder4(input1, input2),
                _paramBuilder5(input1, input2),
                _paramBuilder6(input1, input2),
                _paramBuilder7(input1, input2),
                _paramBuilder8(input1, input2),
                _paramBuilder9(input1, input2)
            );
    }
}