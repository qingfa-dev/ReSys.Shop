using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterOperatorConstantTests
{
    [Fact(DisplayName = "Tokens: All DSL symbol constants match expected values")]
    public void DslTokens_ShouldMatchExpectedValues()
    {
        FilterOperatorConstant.Tokens.Equal.Should().Be("=");
        FilterOperatorConstant.Tokens.EqualCaseSensitive.Should().Be("==");
        FilterOperatorConstant.Tokens.NotEqual.Should().Be("!=");
        FilterOperatorConstant.Tokens.GreaterThan.Should().Be(">");
        FilterOperatorConstant.Tokens.GreaterThanOrEqual.Should().Be(">=");
        FilterOperatorConstant.Tokens.LessThan.Should().Be("<");
        FilterOperatorConstant.Tokens.LessThanOrEqual.Should().Be("<=");
        FilterOperatorConstant.Tokens.Contains.Should().Be("*");
        FilterOperatorConstant.Tokens.ContainsCaseSensitive.Should().Be("*~");
        FilterOperatorConstant.Tokens.NotContains.Should().Be("!*");
        FilterOperatorConstant.Tokens.StartsWith.Should().Be("^");
        FilterOperatorConstant.Tokens.StartsWithCaseSensitive.Should().Be("^~");
        FilterOperatorConstant.Tokens.NotStartsWith.Should().Be("!^");
        FilterOperatorConstant.Tokens.EndsWith.Should().Be("$");
        FilterOperatorConstant.Tokens.EndsWithCaseSensitive.Should().Be("$~");
        FilterOperatorConstant.Tokens.NotEndsWith.Should().Be("!$");
    }

    [Fact(DisplayName = "Tokens: All JSON alias constants match expected values")]
    public void JsonAliases_ShouldMatchExpectedValues()
    {
        FilterOperatorConstant.Tokens.EqualAlias.Should().Be("eq");
        FilterOperatorConstant.Tokens.EqualCaseSensitiveAlias.Should().Be("eq~");
        FilterOperatorConstant.Tokens.NotEqualAlias.Should().Be("neq");
        FilterOperatorConstant.Tokens.GreaterThanAlias.Should().Be("gt");
        FilterOperatorConstant.Tokens.GreaterThanOrEqualAlias.Should().Be("gte");
        FilterOperatorConstant.Tokens.LessThanAlias.Should().Be("lt");
        FilterOperatorConstant.Tokens.LessThanOrEqualAlias.Should().Be("lte");
        FilterOperatorConstant.Tokens.ContainsAlias.Should().Be("contains");
        FilterOperatorConstant.Tokens.ContainsCaseSensitiveAlias.Should().Be("contains~");
        FilterOperatorConstant.Tokens.NotContainsAlias.Should().Be("ncontains");
        FilterOperatorConstant.Tokens.StartsWithAlias.Should().Be("starts");
        FilterOperatorConstant.Tokens.StartsWithCaseSensitiveAlias.Should().Be("starts~");
        FilterOperatorConstant.Tokens.NotStartsWithAlias.Should().Be("nstarts");
        FilterOperatorConstant.Tokens.EndsWithAlias.Should().Be("ends");
        FilterOperatorConstant.Tokens.EndsWithCaseSensitiveAlias.Should().Be("ends~");
        FilterOperatorConstant.Tokens.NotEndsWithAlias.Should().Be("nends");
    }

    [Fact(DisplayName = "Tokens: Lookup token constants match expected values")]
    public void LookupTokens_ShouldMatchExpectedValues()
    {
        FilterOperatorConstant.Tokens.EqualCaseSensitiveLookup.Should().Be("eq~");
        FilterOperatorConstant.Tokens.ContainsCaseSensitiveLookup.Should().Be("contains~");
        FilterOperatorConstant.Tokens.StartsWithCaseSensitiveLookup.Should().Be("starts~");
        FilterOperatorConstant.Tokens.EndsWithCaseSensitiveLookup.Should().Be("ends~");
    }

    [Fact(DisplayName = "Tokens: All DSL symbols are parseable via FilterOperatorMap.TryParse")]
    public void DslTokens_ShouldBeParseable()
    {
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.Equal, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.EqualCaseSensitive, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.NotEqual, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.Contains, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.StartsWith, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.EndsWith, out _).Should().BeTrue();
    }

    [Fact(DisplayName = "Tokens: All JSON aliases are parseable via FilterOperatorMap.TryParse")]
    public void JsonAliases_ShouldBeParseable()
    {
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.EqualAlias, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.NotEqualAlias, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.GreaterThanAlias, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.LessThanAlias, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.ContainsAlias, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.StartsWithAlias, out _).Should().BeTrue();
        FilterOperatorMap.TryParse(FilterOperatorConstant.Tokens.EndsWithAlias, out _).Should().BeTrue();
    }
}
