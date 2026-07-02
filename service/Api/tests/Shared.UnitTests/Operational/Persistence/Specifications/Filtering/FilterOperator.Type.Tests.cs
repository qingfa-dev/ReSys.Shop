using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterOperatorTypeTests
{
    [Fact(DisplayName = "FilterOperator: Should have exactly 16 members")]
    public void ShouldHaveExactlySixteenMembers()
    {
        Enum.GetValues<FilterOperator>().Should().HaveCount(16);
    }

    [Fact(DisplayName = "FilterOperator: All expected members exist")]
    public void AllExpectedMembers_ShouldExist()
    {
        Enum.IsDefined(FilterOperator.Equal).Should().BeTrue();
        Enum.IsDefined(FilterOperator.EqualCaseSensitive).Should().BeTrue();
        Enum.IsDefined(FilterOperator.NotEqual).Should().BeTrue();
        Enum.IsDefined(FilterOperator.GreaterThan).Should().BeTrue();
        Enum.IsDefined(FilterOperator.GreaterThanOrEqual).Should().BeTrue();
        Enum.IsDefined(FilterOperator.LessThan).Should().BeTrue();
        Enum.IsDefined(FilterOperator.LessThanOrEqual).Should().BeTrue();
        Enum.IsDefined(FilterOperator.Contains).Should().BeTrue();
        Enum.IsDefined(FilterOperator.ContainsCaseSensitive).Should().BeTrue();
        Enum.IsDefined(FilterOperator.NotContains).Should().BeTrue();
        Enum.IsDefined(FilterOperator.StartsWith).Should().BeTrue();
        Enum.IsDefined(FilterOperator.StartsWithCaseSensitive).Should().BeTrue();
        Enum.IsDefined(FilterOperator.NotStartsWith).Should().BeTrue();
        Enum.IsDefined(FilterOperator.EndsWith).Should().BeTrue();
        Enum.IsDefined(FilterOperator.EndsWithCaseSensitive).Should().BeTrue();
        Enum.IsDefined(FilterOperator.NotEndsWith).Should().BeTrue();
    }
}
