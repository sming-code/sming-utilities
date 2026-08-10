using System.Diagnostics.CodeAnalysis;

namespace SmingCode.Utilities.Messaging;

internal class CustomPropertyHandler(
    Dictionary<string, object?>? customProperties = null
) : ICustomPropertyHandler
{
    private readonly Dictionary<string, object?> _customProperties = customProperties ?? [];

    public bool TryAddCustomProperty<T>(
        string key,
        T value
    ) => _customProperties.TryAdd(
        key,
        value
    );

    public bool TryGetCustomProperty<T>(
        string key,
        [NotNullWhen(true)] out T? value
    )
    {
        if (_customProperties.TryGetValue(
            key,
            out var valueObject
        ) && valueObject is T valueTyped)
        {
            value = valueTyped;

            return true;
        }

        value = default;
        return false;
    }
}
