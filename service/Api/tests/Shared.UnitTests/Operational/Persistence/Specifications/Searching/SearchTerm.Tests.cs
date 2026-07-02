using Shared.Operational.Persistence.Specifications.Searching;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchTermTests
{
    [Fact(DisplayName = "SearchTerm: Same Value and CaseSensitive are equal")]
    public void SameValueAndCaseSensitive_ShouldBeEqual()
    {
        SearchTerm a = new("hello", false);
        SearchTerm b = new("hello", false);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Theory(DisplayName = "SearchTerm: Not equal when properties differ")]
    [InlineData("hello", false, "world", false)]
    [InlineData("hello", false, "hello", true)]
    public void NotEqual_WhenPropertyDiffers(String valueA, Boolean caseSensitiveA, String valueB, Boolean caseSensitiveB)
    {
        SearchTerm a = new(valueA, caseSensitiveA);
        SearchTerm b = new(valueB, caseSensitiveB);

        a.Should().NotBe(b);
    }

    [Fact(DisplayName = "SearchTerm: Default CaseSensitive is false")]
    public void DefaultCaseSensitive_ShouldBeFalse()
    {
        SearchTerm term = new("hello");

        term.CaseSensitive.Should().BeFalse();
    }
}