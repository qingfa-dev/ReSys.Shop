using Shared.Operational.Persistence.Specifications.Searching;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchTermBehaviorTests
{
    [Theory(DisplayName = "EffectiveValue: Returns ToLowerInvariant when case-insensitive")]
    [InlineData("Hello", "hello")]
    [InlineData("HELLO", "hello")]
    [InlineData("WORLD", "world")]
    public void EffectiveValue_WhenCaseInsensitive_ShouldReturnToLowerInvariant(string value, string expected)
    {
        SearchTerm term = new() { Value = value, CaseSensitive = false };

        term.EffectiveValue.Should().Be(expected);
    }

    [Theory(DisplayName = "EffectiveValue: Returns original value when case-sensitive")]
    [InlineData("Hello")]
    [InlineData("HELLO")]
    [InlineData("world")]
    public void EffectiveValue_WhenCaseSensitive_ShouldReturnOriginal(string value)
    {
        SearchTerm term = new() { Value = value, CaseSensitive = true };

        term.EffectiveValue.Should().Be(value);
    }

    [Theory(DisplayName = "ToString: Appends '~' when case-insensitive")]
    [InlineData("Hello", "Hello~")]
    [InlineData("world", "world~")]
    public void ToString_WhenCaseInsensitive_ShouldAppendTilde(string value, string expected)
    {
        SearchTerm term = new() { Value = value, CaseSensitive = false };

        term.ToString().Should().Be(expected);
    }

    [Theory(DisplayName = "ToString: Returns Value only when case-sensitive")]
    [InlineData("Hello")]
    [InlineData("world")]
    public void ToString_WhenCaseSensitive_ShouldReturnValueOnly(string value)
    {
        SearchTerm term = new() { Value = value, CaseSensitive = true };

        term.ToString().Should().Be(value);
    }
}