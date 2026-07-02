using Shared.Operational.Persistence.Configurations.DateTimes;

namespace Shared.UnitTests.Operational.Persistence.Configurations.DateTimes;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class DateTimeConstantTests
{
    public class Types
    {
        [Fact(DisplayName = "Should have correct Npgsql type string")]
        public void Npgsql_ShouldBeTimestampWithTimeZone()
        {
            DateTimeConstant.Types.Npgsql.Should().Be("timestamp with time zone");
        }
    }
}
