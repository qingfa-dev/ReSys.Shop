namespace Module.Catalog.Domain.OptionTypes;

public static partial class OptionTypeLoggers
{
    #region Management
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "[OptionType.Created]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "[OptionType.Updated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "[OptionType.Deleted]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, string Name, Guid Id, string? ActionBy = "System");
    #endregion
}
