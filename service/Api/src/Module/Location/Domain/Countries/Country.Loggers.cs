namespace Module.Location.Domain.Countries;

/// <summary>LoggerMessage source generators for Country domain events.</summary>
public static partial class CountryLoggers
{
    /// <summary>Logs when a new country is created.</summary>
    [LoggerMessage(EventId = 2101, Level = LogLevel.Debug, Message = "[Country.Created]: {Name} ({IsoCode})")]
    public static partial void Created(ILogger logger, string Name, string IsoCode);

    /// <summary>Logs when a country is updated.</summary>
    [LoggerMessage(EventId = 2102, Level = LogLevel.Debug, Message = "[Country.Updated]: {Name} ({IsoCode})")]
    public static partial void Updated(ILogger logger, string Name, string IsoCode);

    /// <summary>Logs when a country is activated.</summary>
    [LoggerMessage(EventId = 2103, Level = LogLevel.Debug, Message = "[Country.Activated]: {Name}")]
    public static partial void Activated(ILogger logger, string Name);

    /// <summary>Logs when a country is deactivated.</summary>
    [LoggerMessage(EventId = 2104, Level = LogLevel.Debug, Message = "[Country.Deactivated]: {Name}")]
    public static partial void Deactivated(ILogger logger, string Name);
}
