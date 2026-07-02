using Shared.Operational.Persistence.Specifications.Searching;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchTermConstantTests
{
    [Theory(DisplayName = "SearchTerm.Constant: Should have expected values")]
    [InlineData(nameof(SearchTerm.Constant.DefaultCaseSensitive), false)]
    [InlineData(nameof(SearchTerm.Constant.CaseInsensitiveSuffix), "~")]
    [InlineData(nameof(SearchTerm.Constant.DefaultMode), SearchMode.Any)]
    public void Constant_ShouldHaveExpectedValue(String fieldName, Object expectedValue)
    {
        Object actualValue = fieldName switch
        {
            nameof(SearchTerm.Constant.DefaultCaseSensitive) => SearchTerm.Constant.DefaultCaseSensitive,
            nameof(SearchTerm.Constant.CaseInsensitiveSuffix) => SearchTerm.Constant.CaseInsensitiveSuffix,
            nameof(SearchTerm.Constant.DefaultMode) => SearchTerm.Constant.DefaultMode,
            _ => throw new ArgumentOutOfRangeException(nameof(fieldName), fieldName, "Unknown constant field")
        };

        actualValue.Should().Be(expectedValue);
    }
}