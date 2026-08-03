using Shared.Application.Systems.SystemDateTimes;

namespace Shared.UnitTests.Application.Systems.SystemDateTimes;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "SystemDateTime")]
public class SystemDateTimeTests
{
    [Fact(DisplayName = "SystemDateTime should implement ISystemDateTime")]
    public void SystemDateTime_ShouldImplementISystemDateTime()
    {
        var systemDateTime = new SystemDateTime();

        systemDateTime.Should().BeAssignableTo<ISystemDateTime>();
    }

    [Fact(DisplayName = "Constructor should create new instance")]
    public void Constructor_ShouldCreateNewInstance()
    {
        var systemDateTime = new SystemDateTime();

        systemDateTime.Should().NotBeNull();
    }

    [Fact(DisplayName = "UtcNow should return current UTC time")]
    public void UtcNow_ShouldReturnCurrentUtcTime()
    {
        var systemDateTime = new SystemDateTime();
        DateTimeOffset before = DateTime.UtcNow;

        DateTimeOffset result = systemDateTime.UtcNow;

        DateTimeOffset after = DateTime.UtcNow;
        result.Should().BeOnOrAfter(before);
        result.Should().BeOnOrBefore(after);
    }

    [Fact(DisplayName = "UtcNow should have zero offset")]
    public void UtcNow_ShouldReturnZeroOffset()
    {
        var systemDateTime = new SystemDateTime();

        DateTimeOffset result = systemDateTime.UtcNow;

        result.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact(DisplayName = "Today should return today's date in UTC")]
    public void Today_ShouldReturnTodaysDateInUtc()
    {
        var systemDateTime = new SystemDateTime();
        DateTimeOffset expected = DateTime.Today;

        DateTimeOffset result = systemDateTime.Today;

        result.Date.Should().Be(expected.Date);
    }

    [Fact(DisplayName = "Today should have time set to midnight")]
    public void Today_ShouldHaveTimeSetToMidnight()
    {
        var systemDateTime = new SystemDateTime();

        DateTimeOffset result = systemDateTime.Today;

        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }


}
