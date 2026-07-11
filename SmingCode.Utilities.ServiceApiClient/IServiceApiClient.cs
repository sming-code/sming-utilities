namespace SmingCode.Utilities.ServiceApiClient;

public interface IServiceApiClient<TService>
    where TService : class
{
    HttpClient HttpClient { get; }
    Task<ApiClientResponse<TResult>> Get<TResult>(
        string relativeUrl
    ) where TResult : notnull;
    Task<ApiClientResponse> Patch<TRequest>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull;
    Task<ApiClientResponse<TResult>> Patch<TRequest, TResult>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull where TResult : notnull;
    Task<ApiClientResponse> Post<TRequest>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull;
    Task<ApiClientResponse<TResult>> Post<TRequest, TResult>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull where TResult : notnull;
    Task<ApiClientResponse> Put<TRequest>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull;
    Task<ApiClientResponse<TResult>> Put<TRequest, TResult>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull where TResult : notnull;
}
