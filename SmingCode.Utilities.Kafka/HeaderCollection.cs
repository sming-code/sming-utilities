using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace SmingCode.Utilities.Kafka;

public class HeaderCollection : IEnumerable<KeyValuePair<string, string>>
{
    private static readonly JsonSerializerOptions _serializerSettings = JsonSerializerOptions.Web;
    private readonly Dictionary<string, string> _headers = [];

    public HeaderCollection() { }

    internal HeaderCollection(
        Headers kafkaHeaders
    )
    {
        _headers = kafkaHeaders
            .ToDictionary(
                kafkaHeader => kafkaHeader.Key,
                kafkaHeader => Encoding.UTF8.GetString(kafkaHeader.GetValueBytes())
            );
    }

    internal HeaderCollection(
        Dictionary<string, string> headers
    )
    {
        _headers = headers;
    }

    public bool TryGetHeader<T>(
        string key,
        [NotNullWhen(true)] out T? headerValue
    )
    {
        if (!_headers.TryGetValue(key, out var rawHeaderValue))
        {
            headerValue = default;
            return false;
        }

        headerValue = typeof(T) == typeof(string) && rawHeaderValue is T stringVal
            ? stringVal
            : JsonSerializer.Deserialize<T>(rawHeaderValue, _serializerSettings);
        return headerValue is not null;
    }

    public string GetHeader(
        string key
    ) => _headers[key];

    public void Add(
        string key,
        string value
    ) => _headers.Add(key, value);

    public void Add<T>(
        string key,
        T value
    ) => _headers.Add(
        key,
        value is string valueString
            ? valueString
            : JsonSerializer.Serialize(value, _serializerSettings)
    );

    public void Add<T>(
        IEnumerable<KeyValuePair<string, T>> entries
    ) => entries.ToList().ForEach(entry => Add(entry.Key, entry.Value));

    public Headers ToKafkaHeaders() =>
        [
            .. this.Select(header =>
                new Header(header.Key, Encoding.UTF8.GetBytes(header.Value))
            )
        ];

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        => _headers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public HeaderCollection ToNewHeaderCollection(
        params string[]? headersToInclude
    )
    {
        var newHeaders = headersToInclude is null
            ? _headers
            : headersToInclude.ToDictionary(
                headerName => headerName,
                headerName => _headers[headerName]
            );

        return new(newHeaders);
    }
}