namespace Shared.Application.Models.Results;

public readonly partial record struct Result : IResultRecord
{
    public static Result Create(
        bool isSuccess = ResultConstant.DefaultValues.IsSuccess,
        int statusCode = ResultConstant.DefaultValues.StatusCode,
        string? message = null,
        IEnumerable<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => new(
            isSuccess: isSuccess,
            statusCode: statusCode,
            message: message,
            errors: errors?.ToList(),
            metadata: metadata.Length != 0 ? metadata.ToDictionary() : null);

    #region Success

    private static Result FactorySuccess(
        int statusCode = ResultConstant.DefaultValues.StatusCode,
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => Create(
            isSuccess: true,
            statusCode: statusCode,
            message: message,
            metadata: metadata);

    public static Result Ok(
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => FactorySuccess(
            ResultConstant.StatusCodes.Ok,
            message,
            metadata);

    public static Result Created(
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => FactorySuccess(
            ResultConstant.StatusCodes.Created,
            message,
            metadata);

    public static Result Accepted(
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => FactorySuccess(
            ResultConstant.StatusCodes.Accepted,
            message,
            metadata);

    public static Result NoContent(
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => FactorySuccess(
            ResultConstant.StatusCodes.NoContent,
            message,
            metadata);

    #endregion

    #region Failure

    private static Result FactoryFailure(
        int? statusCode = null,
        string? message = null,
        IReadOnlyCollection<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
    {
        IReadOnlyCollection<Error> errorList = errors ?? [];

        return Create(
            isSuccess: false,
          statusCode: statusCode
                ?? errors?.FirstOrDefault().Type
                ?? ResultConstant.StatusCodes.InternalServerError,
            message: message,
            errors: errorList,
            metadata: metadata);
    }

    public static Result BadRequest(
        string? message = null,
        IReadOnlyCollection<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.BadRequest,
            message,
            errors,
            metadata);

    public static Result Unauthorized(
        string? message = null,
        IReadOnlyCollection<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Unauthorized,
            message,
            errors,
            metadata);

    public static Result Forbidden(
        string? message = null,
        IReadOnlyCollection<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Forbidden,
            message,
            errors,
            metadata);

    public static Result NotFound(
        string? message = null,
        IReadOnlyCollection<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.NotFound,
            message,
            errors,
            metadata);

    public static Result Conflict(
        string? message = null,
        IReadOnlyCollection<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Conflict,
            message,
            errors,
            metadata);

    public static Result Validation(
        string? message = null,
        IReadOnlyCollection<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.UnprocessableEntity,
            message,
            errors,
            metadata);

    public static Result Unexpected(
        string? message = null,
        IReadOnlyCollection<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.InternalServerError,
            message,
            errors,
            metadata);

    #endregion

    #region Helpers

    private static Result ToResult(params Error[] errors)
    {
        Error[] list = errors.ToArray();

        return FactoryFailure(
            statusCode: list.FirstOrDefault().Type,
            message: list.FirstOrDefault().Message,
            errors: list);
    }

    public static IResultRecord ToBase(params Error[] errors) => ToResult(errors);

    #endregion

    #region Implicit Operators
    public static implicit operator Result(Error error)
        => ToResult(error);

    public static implicit operator Result(Error[] errors)
        => ToResult(errors);

    public static implicit operator Result(List<Error> errors)
        => ToResult([.. errors]);

    public static implicit operator Result(HashSet<Error> errors)
        => ToResult([.. errors]);

    #endregion
}
public readonly partial record struct Result
{
    public List<Error> Failures => Errors;
    public static Result Failure(Error error) => error;
    public static Result Failure<T>(Error error) => error;
}

public readonly partial record struct Result
{
    public Error FirstFailure => Errors?.FirstOrDefault() ?? default;
}
