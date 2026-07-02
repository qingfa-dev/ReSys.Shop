namespace Shared.Application.Models.Results;

#pragma warning disable CA1000
public readonly partial record struct PagedResult<T> : IResultRecord
{
    #region Methods
    public static PagedResult<T> Create(
          IEnumerable<T>? items = null,
          int page = 1,
          int pageSize = 10,
          long totalCount = 0,
          bool isSuccess = ResultConstant.DefaultValues.IsSuccess,
          int statusCode = ResultConstant.DefaultValues.StatusCode,
          string? message = null,
          IEnumerable<Error>? errors = null,
          params (string Key, object? Value)[] metadata)
          => new()
          {
              Items = items ?? [],
              PageNumber = page,
              PageSize = pageSize,
              TotalCount = totalCount,
              IsSuccess = isSuccess,
              StatusCode = statusCode,
              Message = message,
               Errors = errors?.ToList() ?? [],
               Metadata = metadata.Length != 0 ? metadata.ToDictionary() : null
          };

    #region Success

    public static PagedResult<T> Ok(
        IEnumerable<T> items,
        int page,
        int pageSize,
        long totalCount,
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => Create(
            items: items,
            page: page,
            pageSize: pageSize,
            totalCount: totalCount,
            isSuccess: true,
            statusCode: ResultConstant.StatusCodes.Ok,
            message: message,
            metadata: metadata);

    public static PagedResult<T> Created(
        IEnumerable<T> items,
        int page,
        int pageSize,
        long totalCount,
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => Create(
            items: items,
            page: page,
            pageSize: pageSize,
            totalCount: totalCount,
            isSuccess: true,
            statusCode: ResultConstant.StatusCodes.Created,
            message: message,
            metadata: metadata);

    public static PagedResult<T> Accepted(
        IEnumerable<T> items,
        int page,
        int pageSize,
        long totalCount,
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => Create(
            items: items,
            page: page,
            pageSize: pageSize,
            totalCount: totalCount,
            isSuccess: true,
            statusCode: ResultConstant.StatusCodes.Accepted,
            message: message,
            metadata: metadata);

    public static PagedResult<T> NoContent(
        string? message = null,
        params (string Key, object? Value)[] metadata)
        => Create(
            isSuccess: true,
            statusCode: ResultConstant.StatusCodes.NoContent,
            message: message,
            metadata: metadata);

    #endregion

    #region Failure

    private static PagedResult<T> FactoryFailure(
        int? statusCode = null,
        string? message = null,
        IEnumerable<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
    {
        return Create(
            isSuccess: false,
            statusCode: statusCode
                ?? errors?.FirstOrDefault().Type
                ?? ResultConstant.StatusCodes.InternalServerError,
            message: message,
            errors: errors ?? [],
            metadata: metadata);
    }

    public static PagedResult<T> Validation(
        string? message = null,
        IEnumerable<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.UnprocessableEntity,
            message,
            errors,
            metadata);

    public static PagedResult<T> NotFound(
        string? message = null,
        IEnumerable<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.NotFound,
            message,
            errors,
            metadata);

    public static PagedResult<T> Conflict(
        string? message = null,
        IEnumerable<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Conflict,
            message,
            errors,
            metadata);

    public static PagedResult<T> Unexpected(
        string? message = null,
        IEnumerable<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.InternalServerError,
            message,
            errors,
            metadata);

    public static PagedResult<T> BadRequest(
        string? message = null,
        IEnumerable<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.BadRequest,
            message,
            errors,
            metadata);

    public static PagedResult<T> Unauthorized(
        string? message = null,
        IEnumerable<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Unauthorized,
            message,
            errors,
            metadata);

    public static PagedResult<T> Forbidden(
        string? message = null,
        IEnumerable<Error>? errors = null,
        params (string Key, object? Value)[] metadata)
        => FactoryFailure(
            ResultConstant.StatusCodes.Forbidden,
            message,
            errors,
            metadata);

    #endregion

    #region Implicit Operators

    public static implicit operator PagedResult<T>(Error error)
        => FactoryFailure(
            statusCode: error.Type,
            message: error.Message,
            errors: [error]);

    public static implicit operator PagedResult<T>(Error[] errors)
        => FactoryFailure(
            errors: errors);

    public static implicit operator PagedResult<T>(List<Error> errors)
        => FactoryFailure(
            errors: errors);

    #endregion
    #endregion
}
#pragma warning restore CA1000
