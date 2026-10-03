using Shared.Application.Extensions.Exceptions;

namespace Shared.Application.Models.Errors;

#pragma warning disable CA1000

public readonly partial struct Error
{
    public static Error FromException(
        Exception exception,
        string code,
        string message,
        int type = ErrorType.Unexpected,
        params (string Key, object? Value)[] metadata)
    {
        (string Key, object? Value)[] exceptionMetadata = exception.ToExceptionMetadata();
        (string Key, object? Value)[] merged;

        if (exceptionMetadata.Length > 0)
        {
            merged = new (string Key, object? Value)[metadata.Length + exceptionMetadata.Length];
            Array.Copy(metadata, merged, metadata.Length);
            Array.Copy(exceptionMetadata, 0, merged, metadata.Length, exceptionMetadata.Length);
        }
        else
        {
            merged = metadata;
        }

        return Create(code, message, type, merged);
    }
}

#pragma warning restore CA1000
