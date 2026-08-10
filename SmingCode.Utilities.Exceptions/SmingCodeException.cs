namespace SmingCode.Utilities.Exceptions;

public abstract class SmingCodeException : Exception
{
    public bool IsRetryable { get; }

    public SmingCodeException(
        string? message,
        bool isRetryable
    ) : base(message) => IsRetryable = isRetryable;

    public SmingCodeException(
        string? message,
        Exception innerException,
        bool isRetryable
    ) : base(
        message,
        innerException
    ) => IsRetryable = isRetryable;
}