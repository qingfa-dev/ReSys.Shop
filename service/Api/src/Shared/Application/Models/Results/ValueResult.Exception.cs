using Shared.Application.Extensions.Exceptions;

namespace Shared.Application.Models.Results;

#pragma warning disable CA1000

public readonly partial record struct Result<T>
{
    public static Result<T> BadRequest(
        Exception exception,
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.BadRequest,
            message,
            errors,
            MergeExceptionMetadata(exception, metadata));

    public static Result<T> Unauthorized(
        Exception exception,
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Unauthorized,
            message,
            errors,
            MergeExceptionMetadata(exception, metadata));

    public static Result<T> Forbidden(
        Exception exception,
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Forbidden,
            message,
            errors,
            MergeExceptionMetadata(exception, metadata));

    public static Result<T> NotFound(
        Exception exception,
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.NotFound,
            message,
            errors,
            MergeExceptionMetadata(exception, metadata));

    public static Result<T> Conflict(
        Exception exception,
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Conflict,
            message,
            errors,
            MergeExceptionMetadata(exception, metadata));

    public static Result<T> Validation(
        Exception exception,
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.UnprocessableEntity,
            message,
            errors,
            MergeExceptionMetadata(exception, metadata));

    public static Result<T> Unexpected(
        Exception exception,
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.InternalServerError,
            message,
            errors,
            MergeExceptionMetadata(exception, metadata));

    private static (string Key, object? Value)[] MergeExceptionMetadata(
        Exception exception,
        params (string Key, object? Value)[] metadata)
    {
        (string Key, object? Value)[] exceptionMetadata = exception.ToExceptionMetadata();
        if (exceptionMetadata.Length == 0)
            return metadata;

        var merged = new (string Key, object? Value)[metadata.Length + exceptionMetadata.Length];
        Array.Copy(metadata, merged, metadata.Length);
        Array.Copy(exceptionMetadata, 0, merged, metadata.Length, exceptionMetadata.Length);
        return merged;
    }
}

#pragma warning restore CA1000
