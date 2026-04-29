using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace SmingCode.Utilities.ServiceApiClient;
using Config;

internal class ApiClientMessageSender<TBody, TResponse>(
    ILogger<ApiClientMessageSender<TBody, TResponse>> _logger
) where TBody : notnull where TResponse : notnull
{
    public async Task HandleAsync(
        ApiClientSendContext context
    )
    {
        var serviceDisplayName = context.ApiClientConfiguration.ServiceDisplayName;
        HttpRequestMessageDetail requestMessageDetail = new(
            context.HttpMethod,
            context.TargetUrl,
            context.MessageHeaders,
            typeof(TBody) == typeof(NoBody)
                ? null
                : new RequestBody<TBody>(
                    (TBody)context.Body,
                    context.ApiClientConfiguration.JsonSerializerOptions
                )
        );
        LogOutgoingRequest(requestMessageDetail, serviceDisplayName);

        try
        {
            var response = await context.HttpClient.SendAsync(requestMessageDetail.HttpRequestMessage);
            await CheckAndLogResponse(context.TargetUrl, response, serviceDisplayName);

            if (typeof(TResponse) != typeof(NoResponse))
            {
                var responseBody = await response.Content.ReadFromJsonAsync<TResponse>();

                context.SetResponse(responseBody);
            }
        }
        catch (ServiceApiClientException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogHttpClientCallException(ex, context.TargetUrl, serviceDisplayName);

            throw new ServiceApiClientException(
                context.ApiClientConfiguration.ServiceDisplayName,
                $"Unable to process call to {context.TargetUrl}",
                ex
            );
        }
    }

    private void LogOutgoingRequest(
        HttpRequestMessageDetail requestMessageDetail,
        string serviceDisplayName
    )
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Service Api Client targeting service {TargetServiceName} sending request {RequestDetail} - {TraceType}",
                serviceDisplayName,
                requestMessageDetail.LogDetail,
                Constants.UTILITY_TRACE_TYPE
            );
        }
    }

    private async Task CheckAndLogResponse(
        string targetUrl,
        HttpResponseMessage responseMessage,
        string serviceDisplayName,
        bool throwIfUnsuccessful = true
    )
    {
        if (!responseMessage.IsSuccessStatusCode)
        {
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            _logger.LogError(
                "Service Api Client targeting service {TargetServiceName} received unsuccessful response ({StatusCode}) with details: {ResponseContent} - {TraceType}",
                serviceDisplayName,
                responseMessage.StatusCode,
                responseContent,
                Constants.UTILITY_TRACE_TYPE
            );

            if (throwIfUnsuccessful)
            {
                throw new ServiceApiClientException(
                    serviceDisplayName,
                    $"Service Api Client targeting service {serviceDisplayName} received unsuccessful response ({responseMessage.StatusCode}) for call to {targetUrl}.",
                    _responseMessage: responseMessage
                );
            }
        }
        else if (_logger.IsEnabled(LogLevel.Trace))
        {
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            var responseDetails = new
            {
                StatusCode = responseMessage.StatusCode.ToString(),
                Headers = responseMessage.Headers.Select(header =>
                    $"{header.Key}: {string.Join(';', header.Value)}"
                ).ToList(),
                Content = responseContent
            };

            _logger.LogTrace(
                "Service Api Client targeting service {TargetServiceName} received response with details: {ResponseDetails} - {TraceType}",
                serviceDisplayName,
                responseDetails,
                Constants.UTILITY_TRACE_TYPE
            );
        }
    }

    private void LogHttpClientCallException(
        Exception ex,
        string fullUri,
        string serviceDisplayName
    )
    {
        _logger.LogError(
            ex,
            "Service Api Client targeting service {TargetServiceName} at url {TargetUrl} failed - {TraceType}",
            serviceDisplayName,
            fullUri,
            Constants.UTILITY_TRACE_TYPE
        );
    }
}
