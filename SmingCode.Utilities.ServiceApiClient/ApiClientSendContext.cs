namespace SmingCode.Utilities.ServiceApiClient;

public class ApiClientSendContext
{
    internal ApiClientSendContext(
        HttpClient httpClient,
        ApiClientConfiguration apiClientConfiguration,
        Func<ApiClientSendContext, Task> messageSender,
        HttpMethod httpMethod,
        string targetUrl,
        object body,
        Type bodyType,
        Type responseType,
        HeaderEntryCollection messageHeaders,
        IServiceProvider serviceProvider
    ) => (HttpClient, ApiClientConfiguration, MessageSender, HttpMethod, TargetUrl, Body, BodyType, MessageHeaders, ServiceProvider, ResponseType)
         = (httpClient, apiClientConfiguration, messageSender, httpMethod, targetUrl, body, bodyType, messageHeaders, serviceProvider, responseType);

    internal HttpClient HttpClient { get; }
    internal ApiClientConfiguration ApiClientConfiguration { get; }
    internal Func<ApiClientSendContext, Task> MessageSender { get; }
    public HttpMethod HttpMethod { get; }
    public string TargetUrl { get; private set; }
    public object Body { get; }
    public Type BodyType { get; }
    public HeaderEntryCollection MessageHeaders { get; }
    internal IServiceProvider ServiceProvider { get; }
    public Type ResponseType { get; }
    public object? Response { get; private set; }

    public void UpdateTargetUrl(string newTargetUrl) => TargetUrl = newTargetUrl;
    public void SetResponse<T>(T response)
    {
        if (typeof(T) != ResponseType)
        {
            throw new InvalidCastException(
                $"Attempt to update response to type {typeof(T)} when response body type {ResponseType} expected."
            );
        }

        Response = response;
    }
}

public class ApiClientSendContext_Bak<TBody, TResponse>(
    HttpMethod httpMethod,
    string targetUrl,
    TBody body,
    HeaderEntryCollection messageHeaders,
    IServiceProvider serviceProvider
)
{
    public HttpMethod HttpMethod { get; } = httpMethod;
    public string TargetUrl { get; private set; } = targetUrl;
    public TBody Body { get; } = body;
    public HeaderEntryCollection MessageHeaders { get; } = messageHeaders;
    internal IServiceProvider ServiceProvider { get; } = serviceProvider;
    public TResponse? Response { get; private set; }

    public void UpdateTargetUrl(string newTargetUrl) => TargetUrl = newTargetUrl;
    public void SetResponse(TResponse response) => Response = response;
}
