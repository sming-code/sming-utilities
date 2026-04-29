using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ServiceApiClient;

internal class ApiClient<TService>(
    HttpClient _httpClient,
    ApiClientConfiguration<TService> _apiClientConfiguration,
    MiddlewareHandler _middlewareHandler,
    IServiceProvider _serviceProvider,
    ILogger<ApiClient<TService>> _logger
) : IServiceApiClient<TService> where TService : class
{
    private static ApiClientConfiguration? _clientConfiguration;
    private static readonly NoResponse _noResponse = new();
    private ApiClientConfiguration ClientConfiguration
        => _clientConfiguration ??= new(
            _apiClientConfiguration.ServiceDisplayName,
            _apiClientConfiguration.ServiceName,
            _apiClientConfiguration.JsonSerializerOptions
        );

    public HttpClient HttpClient => _httpClient;

    public async Task Post(
        string relativeUrl
    ) => await Send<NoResponse>(
        HttpMethod.Post,
        relativeUrl
    );

    public async Task<TResult> Post<TRequest, TResult>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull where TResult : notnull
        => await Send<TRequest, TResult>(
            HttpMethod.Post,
            relativeUrl,
            request,
            headers
        );

    public async Task<TResponse> Get<TResponse>(
        string relativeUrl
    ) where TResponse : notnull
        => await Send<TResponse>(
            HttpMethod.Get,
            relativeUrl
        );

    private async Task Send(
        HttpMethod httpMethod,
        string relativeUrl,
        HeaderEntryCollection? messageHeaders = null
    ) => await CallPipeline<NoBody, NoResponse>(
        httpMethod,
        relativeUrl,
        new NoBody(),
        messageHeaders
    );

    private async Task Send<TBody>(
        HttpMethod httpMethod,
        string relativeUrl,
        TBody body,
        HeaderEntryCollection? messageHeaders = null
    ) where TBody : notnull
        => await CallPipeline<TBody, NoResponse>(
            httpMethod,
            relativeUrl,
            body,
            messageHeaders
        );

    private async Task<TResponse> Send<TResponse>(
        HttpMethod httpMethod,
        string relativeUrl,
        HeaderEntryCollection? messageHeaders = null
    ) where TResponse : notnull
    {
        var resultantContext = await CallPipeline<NoBody, TResponse>(
            httpMethod,
            relativeUrl,
            new NoBody(),
            messageHeaders
        );

        return resultantContext is TResponse response
            ? response
            : throw new InvalidCastException(
                "Just plain failed"
            );
    }

    private async Task<TResponse> Send<TBody, TResponse>(
        HttpMethod httpMethod,
        string relativeUrl,
        TBody body,
        HeaderEntryCollection? messageHeaders = null
    ) where TBody : notnull where TResponse : notnull
    {
        var resultantContext = await CallPipeline<TBody, TResponse>(
            httpMethod,
            relativeUrl,
            body,
            messageHeaders
        );

        return resultantContext is TResponse response
            ? response
            : throw new InvalidCastException(
                "Just plain failed"
            );
    }

    private async Task<ApiClientSendContext> CallPipeline<TBody, TResponse>(
        HttpMethod httpMethod,
        string relativeUrl,
        TBody body,
        HeaderEntryCollection? messageHeaders = null
    ) where TBody : notnull where TResponse : notnull
    {
        var messageSender = _serviceProvider.GetRequiredService<ApiClientMessageSender<TBody, TResponse>>();

        var context = new ApiClientSendContext(
            _httpClient,
            ClientConfiguration,
            messageSender.HandleAsync,
            httpMethod,
            GetFullUri(relativeUrl),
            body,
            typeof(TBody),
            typeof(TResponse),
            messageHeaders ?? [],
            _serviceProvider
        );

        await _middlewareHandler.RunPipeline(context);
        return context;
    }

    private string GetFullUri(string relativeUrl)
        => $"{HttpClient.BaseAddress}{relativeUrl.TrimStart('/')}";
}
