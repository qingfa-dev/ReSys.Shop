namespace Module.Inventory.Domain.StockLocations;

public static partial class StockLocationLoggers
{
    #region Management
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "[StockLocation.Created]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "[StockLocation.Updated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "[StockLocation.Deleted]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "[StockLocation.Activated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Activated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "[StockLocation.Deactivated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deactivated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Debug,
        Message = "[StockLocation.SetAsDefault]: {Name} ({Id}) by {ActionBy}")]
    public static partial void SetAsDefault(ILogger logger, string Name, Guid Id, string? ActionBy = "System");
    #endregion
}
