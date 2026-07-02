namespace Shared.Application.Models.Errors;

public static class ErrorType
{
    public const int BadRequest = ResultConstant.StatusCodes.BadRequest;
    public const int Unauthorized = ResultConstant.StatusCodes.Unauthorized;
    public const int Forbidden = ResultConstant.StatusCodes.Forbidden;
    public const int NotFound = ResultConstant.StatusCodes.NotFound;
    public const int Conflict = ResultConstant.StatusCodes.Conflict;
    public const int Validation = ResultConstant.StatusCodes.UnprocessableEntity;

    public const int Unexpected = ResultConstant.StatusCodes.InternalServerError;
}