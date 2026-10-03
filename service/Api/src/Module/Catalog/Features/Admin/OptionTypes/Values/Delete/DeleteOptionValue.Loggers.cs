namespace Module.Catalog.Features.Admin.Optiontypes.Values.Delete;

public static partial class DeleteOptionValue
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 7,
            Level = LogLevel.Debug,
            Message = "[OptionValue.Deleted]: {Name} ({Id}) from option type {OptionTypeId} by {ActionBy}")]
        public static partial void Deleted(ILogger logger, Guid Id, Guid OptionTypeId, string Name, string? ActionBy);
    }
}