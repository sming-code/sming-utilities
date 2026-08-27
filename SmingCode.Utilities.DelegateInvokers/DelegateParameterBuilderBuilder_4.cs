using System.Reflection;

namespace SmingCode.Utilities.DelegateInvokers;

public abstract class DelegateParameterBuilderBuilder<TIn1, TIn2, TIn3>
{
    public abstract Func<TIn1, TIn2, TIn3, TParam> BuildParameterBuilder<TParam>(ParameterInfo parameterInfo);
}
