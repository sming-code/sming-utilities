using System.Text.Json;

namespace SmingCode.Utilities.ServiceApiClient;

internal record ApiClientConfiguration<TService>(
    string ServiceDisplayName,
    string ServiceName
)
{
    internal JsonSerializerOptions JsonSerializerOptions { get; } = JsonSerializerOptions.Web;
};

internal record ApiClientConfiguration(
    string ServiceDisplayName,
    string ServiceName,
    JsonSerializerOptions JsonSerializerOptions
);
