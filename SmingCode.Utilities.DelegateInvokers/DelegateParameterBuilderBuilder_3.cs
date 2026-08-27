using System.Reflection;

namespace SmingCode.Utilities.DelegateInvokers;

public abstract class DelegateParameterBuilderBuilder<TIn1, TIn2>
{
    public abstract Func<TIn1, TIn2, TParam> BuildParameterBuilder<TParam>(ParameterInfo parameterInfo);
}
