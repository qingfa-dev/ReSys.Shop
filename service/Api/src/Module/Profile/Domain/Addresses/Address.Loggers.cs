namespace Module.Profile.Domain.Addresses;

public static partial class AddressLoggers
{
    public static partial class Management
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Debug,
            Message = "[Address.Created]: {Address1} ({Id}) by {ActionBy}")]
        public static partial void Created(ILogger logger, string Address1, Guid Id, string? ActionBy = "System");

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Debug,
            Message = "[Address.Updated]: {Address1} ({Id}) by {ActionBy}")]
        public static partial void Updated(ILogger logger, string Address1, Guid Id, string? ActionBy = "System");

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Debug,
            Message = "[Address.Deleted]: {Address1} ({Id}) by {ActionBy}")]
        public static partial void Deleted(ILogger logger, string Address1, Guid Id, string? ActionBy = "System");
    }
}
