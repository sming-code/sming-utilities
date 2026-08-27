namespace SmingCode.Utilities.Exceptions;

public class BadRequestException : SmingCodeException
{
    public BadRequestException(
        string? message,
        bool isRetryable
    ) : base(
        message,
        isRetryable
    )
    { }

    public BadRequestException(
        string? message,
        Exception innerException,
        bool isRetryable
    ) : base(
        message,
        innerException,
        isRetryable
    )
    { }
}