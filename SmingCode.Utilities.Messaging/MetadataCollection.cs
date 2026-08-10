using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace SmingCode.Utilities.Messaging;

public class MetadataCollection : IEnumerable<KeyValuePair<string, string>>
{
    private static readonly JsonSerializerOptions _serializerSettings = JsonSerializerOptions.Web;
    private readonly Dictionary<string, string> _metadataSet = [];

    public MetadataCollection() { }

    internal MetadataCollection(
        Dictionary<string, string> metadataSet
    )
    {
        _metadataSet = metadataSet;
    }

    public bool TryGetMetadata<T>(
        string key,
        [NotNullWhen(true)] out T? metadataValue
    )
    {
        if (!_metadataSet.TryGetValue(key, out var rawMetadataValue))
        {
            metadataValue = default;
            return false;
        }

        metadataValue = typeof(T) == typeof(string) && rawMetadataValue is T stringVal
            ? stringVal
            : JsonSerializer.Deserialize<T>(rawMetadataValue, _serializerSettings);
        return metadataValue is not null;
    }

    public string GetMetadata(
        string key
    ) => _metadataSet[key];

    public void Add(
        string key,
        string value
    ) => _metadataSet.Add(key, value);

    public void Add<T>(
        string key,
        T value
    ) => _metadataSet.Add(
        key,
        value is string valueString
            ? valueString
            : JsonSerializer.Serialize(value, _serializerSettings)
    );

    public void Add<T>(
        IEnumerable<KeyValuePair<string, T>> entries
    ) => entries.ToList().ForEach(entry => Add(entry.Key, entry.Value));

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        => _metadataSet.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public MetadataCollection ToNewMetadataCollection(
        params string[]? metadataEntriesToInclude
    )
    {
        var newMetadataEntries = metadataEntriesToInclude is null
            ? _metadataSet
            : metadataEntriesToInclude.ToDictionary(
                metadataEntryName => metadataEntryName,
                metadataEntryName => _metadataSet[metadataEntryName]
            );

        return new(newMetadataEntries);
    }
}