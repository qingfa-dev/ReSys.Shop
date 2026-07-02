namespace Shared.Security.Authentication.Tokens.Services.Refresh.Store;

public partial class RefreshTokenStore
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 300,
            Level = LogLevel.Error,
            Message = "Refresh token store operation '{Operation}' failed")]
        public static partial void LogStoreOperationFailed(ILogger logger, string operation, Exception ex);
    }
}
