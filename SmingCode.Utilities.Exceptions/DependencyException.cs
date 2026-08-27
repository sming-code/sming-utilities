namespace SmingCode.Utilities.Exceptions;

public class DependencyException : SmingCodeException
{
    public DependencyException(
        string dependencyName,
        string? message,
        bool isRetryable
    ) : base(
        $"Exception occurred in dependency {dependencyName} - {message}",
        isRetryable
    )
    { }

    public DependencyException(
        string dependencyName,
        string? message,
        Exception innerException,
        bool isRetryable
    ) : base(
        $"Exception occurred in dependency {dependencyName} - {message}",
        innerException,
        isRetryable
    )
    { }
}