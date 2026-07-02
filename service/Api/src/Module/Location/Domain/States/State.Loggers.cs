namespace Module.Location.Domain.States;

public static partial class StateLoggers
{
    [LoggerMessage(EventId = 2201, Level = LogLevel.Debug, Message = "[State.Created]: {Name} ({Abbreviation}) in Country {CountryId}")]
    public static partial void Created(ILogger logger, string Name, string Abbreviation, Guid CountryId);

    [LoggerMessage(EventId = 2202, Level = LogLevel.Debug, Message = "[State.Updated]: {Name} ({Abbreviation}) in Country {CountryId}")]
    public static partial void Updated(ILogger logger, string Name, string Abbreviation, Guid CountryId);

    [LoggerMessage(EventId = 2203, Level = LogLevel.Debug, Message = "[State.Activated]: {Name}")]
    public static partial void Activated(ILogger logger, string Name);

    [LoggerMessage(EventId = 2204, Level = LogLevel.Debug, Message = "[State.Deactivated]: {Name}")]
    public static partial void Deactivated(ILogger logger, string Name);
}
