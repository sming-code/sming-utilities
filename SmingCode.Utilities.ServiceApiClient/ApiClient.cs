using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ServiceApiClient;
using Config;

internal class ApiClient<TService>(
    HttpClient _httpClient,
    ApiClientDetail<TService> _apiClientDetail,
    MiddlewareHandler<TService> _middlewareHandler,
    IServiceProvider _serviceProvider,
    ILogger<ApiClient<TService>> _logger
) : IServiceApiClient<TService> where TService : class
{
    public HttpClient HttpClient => _httpClient;

    public async Task<ApiClientResponse<TResponse>> Get<TResponse>(
        string relativeUrl
    ) where TResponse : notnull
        => await Send<TResponse>(
            HttpMethod.Get,
            relativeUrl
        );

    public async Task<ApiClientResponse> Patch<TRequest>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull
        => await Send(
            HttpMethod.Patch,
            relativeUrl,
            request,
            headers
        );

    public async Task<ApiClientResponse<TResult>> Patch<TRequest, TResult>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull where TResult : notnull
        => await Send<TRequest, TResult>(
            HttpMethod.Patch,
            relativeUrl,
            request,
            headers
        );

    public async Task<ApiClientResponse> Post<TRequest>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull
        => await Send(
            HttpMethod.Post,
            relativeUrl,
            request,
            headers
        );

    public async Task<ApiClientResponse<TResult>> Post<TRequest, TResult>(
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

    public async Task<ApiClientResponse> Put<TRequest>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull
        => await Send(
            HttpMethod.Put,
            relativeUrl,
            request,
            headers
        );

    public async Task<ApiClientResponse<TResult>> Put<TRequest, TResult>(
        string relativeUrl,
        TRequest request,
        HeaderEntryCollection? headers = null
    ) where TRequest : notnull where TResult : notnull
        => await Send<TRequest, TResult>(
            HttpMethod.Put,
            relativeUrl,
            request,
            headers
        );

    private async Task<ApiClientResponse> Send(
        HttpMethod httpMethod,
        string relativeUrl,
        HeaderEntryCollection? messageHeaders = null
    )
    {
        var resultantContext = await CallPipeline<NoBody, NoResponse>(
            httpMethod,
            relativeUrl,
            new NoBody(),
            messageHeaders
        );

        return resultantContext.Response is ApiClientResponse response
            ? response
            : throw new InvalidCastException(
                "Just plain failed"
            );
    }

    private async Task<ApiClientResponse> Send<TBody>(
        HttpMethod httpMethod,
        string relativeUrl,
        TBody body,
        HeaderEntryCollection? messageHeaders = null
    ) where TBody : notnull
    {
        var resultantContext = await CallPipeline<TBody, NoResponse>(
            httpMethod,
            relativeUrl,
            body,
            messageHeaders
        );

        return resultantContext.Response is ApiClientResponse response
            ? response
            : throw new InvalidCastException(
                "Just plain failed"
            );
    }

    private async Task<ApiClientResponse<TResponse>> Send<TResponse>(
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

        return resultantContext.Response is ApiClientResponse<TResponse> response
            ? response
            : throw new InvalidCastException(
                "Just plain failed"
            );
    }

    private async Task<ApiClientResponse<TResponse>> Send<TBody, TResponse>(
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

        return resultantContext.Response is ApiClientResponse<TResponse> response
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
        var fullUri = GetFullUri(relativeUrl);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Processing call to {TargetUrl} ({HttpMethod}) - {TraceType} - {TargetServiceName}",
                fullUri,
                httpMethod,
                Constants.UTILITY_TRACE_TYPE,
                _apiClientDetail.ServiceDisplayName
            );
        }

        var messageSender = _serviceProvider.GetRequiredService<ApiClientMessageSender<TBody, TResponse>>();

        var context = new ApiClientSendContext(
            _httpClient,
            _apiClientDetail,
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

