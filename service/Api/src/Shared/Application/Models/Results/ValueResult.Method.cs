namespace Shared.Application.Models.Results;

#pragma warning disable CA1000
public readonly partial record struct Result<T> : IResultRecord
{
    #region Methods
    #region Factory
    public static Result<T> Create(
        bool isSuccess = ResultConstant.DefaultValues.IsSuccess,
        int statusCode = ResultConstant.DefaultValues.StatusCode,
        T? value = default,
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => new(isSuccess, statusCode, value, message, errors, metadata);
    private static Result<T> FactorySuccess(
        T value,
        int statusCode,
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => Create(
            isSuccess: true,
            statusCode: statusCode,
            value: value,
            message: message,
            metadata: metadata);
    private static Result<T> FactoryFailure(
        int? statusCode = null,
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
    {
        return Create(
            value: default,
            isSuccess: false,
            statusCode: statusCode
                ?? errors?.FirstOrDefault().Type
                ?? ResultConstant.StatusCodes.InternalServerError,
            message: message,
            errors: errors,
            metadata: metadata);
    }
    #endregion

    #region Success
    public static Result<T> Ok(
        T value,
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => FactorySuccess(
            value,
            ResultConstant.StatusCodes.Ok,
            message,
            metadata);

    public static Result<T> Created(
        T value,
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => FactorySuccess(
            value,
            ResultConstant.StatusCodes.Created,
            message,
            metadata);

    public static Result<T> Accepted(
        T value,
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => FactorySuccess(
            value: value,
            statusCode: ResultConstant.StatusCodes.Accepted,
            message: message,
            metadata: metadata);

    public static Result<T> NoContent(
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => FactorySuccess(
            default!,
            ResultConstant.StatusCodes.NoContent,
            message,
            metadata);
    #endregion

    #region Failure
    public static Result<T> BadRequest(
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.BadRequest,
            message,
            errors,
            metadata);

    public static Result<T> Unauthorized(
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Unauthorized,
            message,
            errors,
            metadata);

    public static Result<T> Forbidden(
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Forbidden,
            message,
            errors,
            metadata);

    public static Result<T> NotFound(
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.NotFound,
            message,
            errors,
            metadata);

    public static Result<T> Conflict(
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Conflict,
            message,
            errors,
            metadata);

    public static Result<T> Validation(
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.UnprocessableEntity,
            message,
            errors,
            metadata);

    public static Result<T> Unexpected(
        string? message = null,
        List<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.InternalServerError,
            message,
            errors,
            metadata);

    #endregion

    #region Conversions

    public Result ToBase() => Result.Create(
        IsSuccess,
        StatusCode,
        Message,
        Errors,
        Metadata?.Select(kv => (kv.Key, kv.Value)).ToArray() ?? []);

    #endregion

    #region Implicit Operators

    public static implicit operator Result<T>(T value)
        => Ok(value);

    public static implicit operator Result<T>(Error error)
        => FactoryFailure(
            statusCode: error.Type,
            message: error.Message,
            errors: [error]);

    public static implicit operator Result<T>(Error[] errors)
        => FactoryFailure(
            errors: [.. errors]);

    public static implicit operator Result<T>(List<Error> errors)
        => FactoryFailure(
            errors: errors);

    #endregion
    #endregion
}
#pragma warning restore CA1000
