namespace Shared.Application.Systems.SystemDateTimes;

public sealed class SystemDateTime : ISystemDateTime
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateTimeOffset Today => DateTime.Today;
}
