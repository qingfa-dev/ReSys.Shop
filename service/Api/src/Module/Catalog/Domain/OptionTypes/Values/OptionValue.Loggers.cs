namespace Module.Catalog.Domain.OptionTypes.Values;

public static partial class OptionValueLoggers
{
    #region Management
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "[OptionValue.Created]: {Name} ({Id}) for option type {OptionTypeId} by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, Guid OptionTypeId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Debug,
        Message = "[OptionValue.Updated]: {Name} ({Id}) for option type {OptionTypeId} by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Name, Guid Id, Guid OptionTypeId, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Debug,
        Message = "[OptionValue.Deleted]: {Name} ({Id}) from option type {OptionTypeId} by {ActionBy}")]
    public static partial void Deleted(ILogger logger, string Name, Guid Id, Guid OptionTypeId, string? ActionBy = "System");
    #endregion
}