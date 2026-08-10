using System.Reflection;

namespace SmingCode.Utilities.DelegateInvokers;

public abstract class DelegateParameterBuilderBuilder
{
    public abstract Func<TParam> BuildParameterBuilder<TParam>(ParameterInfo parameterInfo);
}
