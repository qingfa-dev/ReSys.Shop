using Http = Microsoft.AspNetCore.Http;

namespace Shared.Application.Extensions.Results;

public static class ResultHttpExtensions
{
    #region Public Methods

    public static Http.IResult ToResult(this Result result)
        => result.IsSuccess ? MapSuccessResult(result) : MapFailureResult(result);

    public static Http.IResult ToResult<T>(this Result<T> result)
        => result.IsSuccess ? MapSuccessResult(result) : MapFailureResult(result);

    public static Http.IResult ToPagedResult<T>(this PagedResult<T> result)
        => result.IsSuccess ? MapSuccessResult(result) : MapFailureResult(result);

    public static Http.IResult ToCreatedResult<T>(this Result<T> result, string locationUri)
        => result.IsSuccess
            ? Http.TypedResults.Created(locationUri, result)
            : MapFailureResult(result);

    public static Http.IResult ToAcceptedResult<T>(this Result<T> result, string? locationUri = null)
        => result.IsSuccess
            ? Http.TypedResults.Accepted(locationUri ?? string.Empty, result)
            : MapFailureResult(result);

    public static Http.IResult ToAcceptedResult(this Result result, string? locationUri = null)
        => result.IsSuccess
            ? Http.TypedResults.Accepted(locationUri ?? string.Empty, result)
            : MapFailureResult(result);

    public static Http.IResult ToNoContentResult(this Result result)
        => result.IsSuccess ? Http.TypedResults.NoContent() : MapFailureResult(result);

    public static Http.IResult ToNoContentResult<T>(this Result<T> result)
        => result.IsSuccess ? Http.TypedResults.NoContent() : MapFailureResult(result);

    public static Http.IResult ToApiResult(this Result result)
        => result.ToResult();

    public static Http.IResult ToApiResult<T>(this Result<T> result)
        => result.ToResult();

    #endregion

    #region Private Mapping Methods

    private static Http.IResult MapSuccessResult(Result result)
    {
        return result.StatusCode switch
        {
            Http.StatusCodes.Status200OK => Http.TypedResults.Ok(result),
            Http.StatusCodes.Status201Created => Http.TypedResults.Created(string.Empty, result),
            Http.StatusCodes.Status202Accepted => Http.TypedResults.Accepted(string.Empty, result),
            Http.StatusCodes.Status204NoContent => Http.TypedResults.NoContent(),
            _ => Http.TypedResults.Ok(result)
        };
    }

    private static Http.IResult MapSuccessResult<T>(Result<T> result)
    {
        return result.StatusCode switch
        {
            Http.StatusCodes.Status200OK => Http.TypedResults.Ok(result),
            Http.StatusCodes.Status201Created => Http.TypedResults.Created(string.Empty, result),
            Http.StatusCodes.Status202Accepted => Http.TypedResults.Accepted(string.Empty, result),
            Http.StatusCodes.Status204NoContent => Http.TypedResults.NoContent(),
            _ => Http.TypedResults.Ok(result)
        };
    }

    private static Http.IResult MapSuccessResult<T>(PagedResult<T> result)
    {
        return result.StatusCode switch
        {
            Http.StatusCodes.Status200OK => Http.TypedResults.Ok(result),
            Http.StatusCodes.Status201Created => Http.TypedResults.Created(string.Empty, result),
            Http.StatusCodes.Status202Accepted => Http.TypedResults.Accepted(string.Empty, result),
            Http.StatusCodes.Status204NoContent => Http.TypedResults.NoContent(),
            _ => Http.TypedResults.Ok(result)
        };
    }

    private static Http.IResult MapFailureResult<TResult>(TResult result)
        where TResult : IResultRecord
    {
        int statusCode = result.StatusCode == 0
            ? Http.StatusCodes.Status400BadRequest
            : result.StatusCode;

        // Convert to non-generic Result to avoid serializing Value property on generic failures
        Result safeResult = new(
            isSuccess: result.IsSuccess,
            statusCode: result.StatusCode,
            message: result.Message,
            errors: result.Errors,
            metadata: result.Metadata);

        return statusCode switch
        {
            Http.StatusCodes.Status400BadRequest => Http.TypedResults.BadRequest(safeResult),
            Http.StatusCodes.Status401Unauthorized => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status401Unauthorized),
            Http.StatusCodes.Status403Forbidden => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status403Forbidden),
            Http.StatusCodes.Status404NotFound => Http.TypedResults.NotFound(safeResult),
            Http.StatusCodes.Status405MethodNotAllowed => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status405MethodNotAllowed),
            Http.StatusCodes.Status406NotAcceptable => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status406NotAcceptable),
            Http.StatusCodes.Status408RequestTimeout => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status408RequestTimeout),
            Http.StatusCodes.Status409Conflict => Http.TypedResults.Conflict(safeResult),
            Http.StatusCodes.Status412PreconditionFailed => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status412PreconditionFailed),
            Http.StatusCodes.Status413PayloadTooLarge => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status413PayloadTooLarge),
            Http.StatusCodes.Status415UnsupportedMediaType => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status415UnsupportedMediaType),
            Http.StatusCodes.Status422UnprocessableEntity => Http.TypedResults.UnprocessableEntity(safeResult),
            Http.StatusCodes.Status429TooManyRequests => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status429TooManyRequests),
            Http.StatusCodes.Status500InternalServerError => Http.TypedResults.InternalServerError(safeResult),
            Http.StatusCodes.Status502BadGateway => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status502BadGateway),
            Http.StatusCodes.Status503ServiceUnavailable => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status503ServiceUnavailable),
            Http.StatusCodes.Status504GatewayTimeout => Http.TypedResults.Json(safeResult, statusCode: Http.StatusCodes.Status504GatewayTimeout),
            _ => Http.TypedResults.Json(safeResult, statusCode: statusCode)
        };
    }

    #endregion
}
