using System.Diagnostics.CodeAnalysis;

namespace SmingCode.Utilities.Messaging;

public interface ICustomPropertyHandler
{
    bool TryAddCustomProperty<T>(
        string key,
        T value
    );
    bool TryGetCustomProperty<T>(
        string key,
        [NotNullWhen(true)] out T? value
    );
}
