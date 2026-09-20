namespace Shared.Application.Models.Results;

public static class ResultConstant
{
    #region Constraints
    public static class Constraints
    {
        public const int MaxErrors = 100;
    }
    #endregion

    #region StatusCodes
    public static class StatusCodes
    {
        // Success
        public const int Ok = 200;
        public const int Created = 201;
        public const int Accepted = 202;
        public const int NoContent = 204;

        // Client Errors
        public const int BadRequest = 400;
        public const int Unauthorized = 401;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int Conflict = 409;
        public const int Gone = 410;
        public const int UnprocessableEntity = 422;
        public const int TooManyRequests = 429;

        // Server Errors
        public const int InternalServerError = 500;
        public const int NotImplemented = 501;
        public const int BadGateway = 502;
        public const int ServiceUnavailable = 503;
        public const int GatewayTimeout = 504;
    }
    #endregion

    #region DefaultValues
    public static class DefaultValues
    {
        public const bool IsSuccess = true;
        public const int StatusCode = StatusCodes.Ok;
    }
    #endregion

    #region Messages
    public static class Messages
    {
        public const string Success = "Operation completed successfully.";
        public const string Failure = "Operation failed.";
    }
    #endregion
}