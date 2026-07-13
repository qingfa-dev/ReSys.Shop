namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Update;

public static partial class UpdateOptionValue
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 6,
            Level = LogLevel.Debug,
            Message = "[OptionValue.Updated]: {Name} ({Id}) for option type {OptionTypeId} by {ActionBy}")]
        public static partial void Updated(ILogger logger, Guid Id, Guid OptionTypeId, string Name, string? ActionBy);
    }
}