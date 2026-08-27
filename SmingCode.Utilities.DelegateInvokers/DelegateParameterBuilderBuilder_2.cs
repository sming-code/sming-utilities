using System.Reflection;

namespace SmingCode.Utilities.DelegateInvokers;

public abstract class DelegateParameterBuilderBuilder<TIn>
{
    public abstract Func<TIn, TParam> BuildParameterBuilder<TParam>(ParameterInfo parameterInfo);
}
