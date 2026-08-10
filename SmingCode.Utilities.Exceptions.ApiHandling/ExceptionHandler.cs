using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace SmingCode.Utilities.Exceptions.ApiHandling;

public class ExceptionHandler(
    ILogger<ExceptionHandler> _logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var endpoint = httpContext.Request.Method;
        _logger.LogError(
            exception,
            "Error occurred processing {Endpoint}",
            endpoint
        );

        var responseCode = exception switch
        {
            BadRequestException => HttpStatusCode.BadRequest,
            DependencyException => HttpStatusCode.FailedDependency,
            _ => HttpStatusCode.InternalServerError
        };

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = (int)responseCode;
        var body = new
        {
            Message = "An error occurred whilst processing your request."
        };
        var bodyJson = JsonSerializer.Serialize(body);
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(bodyJson, cancellationToken);

        return await Task.FromResult(true);
    }
}