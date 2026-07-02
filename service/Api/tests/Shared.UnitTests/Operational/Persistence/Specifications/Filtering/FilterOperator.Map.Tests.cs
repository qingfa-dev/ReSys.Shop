using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterOperatorMapTests
{
    #region ToDslToken (parameterized)

    [Theory(DisplayName = "ToDslToken: Returns canonical DSL token for each operator")]
    [InlineData(FilterOperator.Equal, "=")]
    [InlineData(FilterOperator.EqualCaseSensitive, "==")]
    [InlineData(FilterOperator.NotEqual, "!=")]
    [InlineData(FilterOperator.GreaterThan, ">")]
    [InlineData(FilterOperator.GreaterThanOrEqual, ">=")]
    [InlineData(FilterOperator.LessThan, "<")]
    [InlineData(FilterOperator.LessThanOrEqual, "<=")]
    [InlineData(FilterOperator.Contains, "*")]
    [InlineData(FilterOperator.ContainsCaseSensitive, "*~")]
    [InlineData(FilterOperator.NotContains, "!*")]
    [InlineData(FilterOperator.StartsWith, "^")]
    [InlineData(FilterOperator.StartsWithCaseSensitive, "^~")]
    [InlineData(FilterOperator.NotStartsWith, "!^")]
    [InlineData(FilterOperator.EndsWith, "$")]
    [InlineData(FilterOperator.EndsWithCaseSensitive, "$~")]
    [InlineData(FilterOperator.NotEndsWith, "!$")]
    public void ToDslToken_ShouldReturnCorrectToken(FilterOperator op, string expected)
    {
        FilterOperatorMap.ToDslToken(op).Should().Be(expected);
    }

    [Fact(DisplayName = "ToDslToken: Throws ArgumentOutOfRangeException for invalid operator")]
    public void ToDslToken_ShouldThrow_ForInvalidOperator()
    {
        FilterOperator invalid = (FilterOperator)999;
        Action act = () => FilterOperatorMap.ToDslToken(invalid);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region TryParse (parameterized)

    [Theory(DisplayName = "TryParse: Recognizes all DSL tokens")]
    [InlineData("=", FilterOperator.Equal)]
    [InlineData("==", FilterOperator.EqualCaseSensitive)]
    [InlineData("!=", FilterOperator.NotEqual)]
    [InlineData(">", FilterOperator.GreaterThan)]
    [InlineData(">=", FilterOperator.GreaterThanOrEqual)]
    [InlineData("<", FilterOperator.LessThan)]
    [InlineData("<=", FilterOperator.LessThanOrEqual)]
    [InlineData("*", FilterOperator.Contains)]
    [InlineData("*~", FilterOperator.ContainsCaseSensitive)]
    [InlineData("!*", FilterOperator.NotContains)]
    [InlineData("^", FilterOperator.StartsWith)]
    [InlineData("^~", FilterOperator.StartsWithCaseSensitive)]
    [InlineData("!^", FilterOperator.NotStartsWith)]
    [InlineData("$", FilterOperator.EndsWith)]
    [InlineData("$~", FilterOperator.EndsWithCaseSensitive)]
    [InlineData("!$", FilterOperator.NotEndsWith)]
    public void TryParse_DslTokens_ShouldMap(string token, FilterOperator expected)
    {
        FilterOperatorMap.TryParse(token, out FilterOperator op).Should().BeTrue();
        op.Should().Be(expected);
    }

    [Theory(DisplayName = "TryParse: Recognizes all JSON aliases")]
    [InlineData("eq", FilterOperator.Equal)]
    [InlineData("eq~", FilterOperator.EqualCaseSensitive)]
    [InlineData("neq", FilterOperator.NotEqual)]
    [InlineData("gt", FilterOperator.GreaterThan)]
    [InlineData("gte", FilterOperator.GreaterThanOrEqual)]
    [InlineData("lt", FilterOperator.LessThan)]
    [InlineData("lte", FilterOperator.LessThanOrEqual)]
    [InlineData("contains", FilterOperator.Contains)]
    [InlineData("contains~", FilterOperator.ContainsCaseSensitive)]
    [InlineData("ncontains", FilterOperator.NotContains)]
    [InlineData("starts", FilterOperator.StartsWith)]
    [InlineData("starts~", FilterOperator.StartsWithCaseSensitive)]
    [InlineData("nstarts", FilterOperator.NotStartsWith)]
    [InlineData("ends", FilterOperator.EndsWith)]
    [InlineData("ends~", FilterOperator.EndsWithCaseSensitive)]
    [InlineData("nends", FilterOperator.NotEndsWith)]
    public void TryParse_JsonAliases_ShouldMap(string token, FilterOperator expected)
    {
        FilterOperatorMap.TryParse(token, out FilterOperator op).Should().BeTrue();
        op.Should().Be(expected);
    }

    [Theory(DisplayName = "TryParse: Case-insensitive matching")]
    [InlineData("EQ")]
    [InlineData("Eq")]
    [InlineData("NEQ")]
    [InlineData("CONTAINS")]
    [InlineData("STARTS")]
    [InlineData("ENDS")]
    [InlineData("NContains")]
    public void TryParse_ShouldBeCaseInsensitive(string token)
    {
        FilterOperatorMap.TryParse(token, out _).Should().BeTrue();
    }

    [Fact(DisplayName = "TryParse: Returns false for null")]
    public void TryParse_ShouldReturnFalse_ForNull()
    {
        FilterOperatorMap.TryParse(null, out _).Should().BeFalse();
    }

    [Theory(DisplayName = "TryParse: Returns false for unknown tokens")]
    [InlineData("unknown")]
    [InlineData("??")]
    [InlineData("")]
    [InlineData("===")]
    public void TryParse_UnknownTokens_ShouldReturnFalse(string token)
    {
        FilterOperatorMap.TryParse(token, out _).Should().BeFalse();
    }

    #endregion

    #region Classification (parameterized)

    [Theory(DisplayName = "IsCaseSensitive: True for case-sensitive variants only")]
    [InlineData(FilterOperator.EqualCaseSensitive, true)]
    [InlineData(FilterOperator.ContainsCaseSensitive, true)]
    [InlineData(FilterOperator.StartsWithCaseSensitive, true)]
    [InlineData(FilterOperator.EndsWithCaseSensitive, true)]
    [InlineData(FilterOperator.Equal, false)]
    [InlineData(FilterOperator.NotEqual, false)]
    [InlineData(FilterOperator.Contains, false)]
    [InlineData(FilterOperator.GreaterThan, false)]
    public void IsCaseSensitive_ShouldClassifyCorrectly(FilterOperator op, bool expected)
    {
        FilterOperatorMap.IsCaseSensitive(op).Should().Be(expected);
    }

    [Theory(DisplayName = "IsNegation: True for negation operators only")]
    [InlineData(FilterOperator.NotEqual, true)]
    [InlineData(FilterOperator.NotContains, true)]
    [InlineData(FilterOperator.NotStartsWith, true)]
    [InlineData(FilterOperator.NotEndsWith, true)]
    [InlineData(FilterOperator.Equal, false)]
    [InlineData(FilterOperator.Contains, false)]
    [InlineData(FilterOperator.StartsWith, false)]
    [InlineData(FilterOperator.GreaterThan, false)]
    public void IsNegation_ShouldClassifyCorrectly(FilterOperator op, bool expected)
    {
        FilterOperatorMap.IsNegation(op).Should().Be(expected);
    }

    [Theory(DisplayName = "IsStringOnly: True for string-pattern operators only")]
    [InlineData(FilterOperator.Contains, true)]
    [InlineData(FilterOperator.ContainsCaseSensitive, true)]
    [InlineData(FilterOperator.NotContains, true)]
    [InlineData(FilterOperator.StartsWith, true)]
    [InlineData(FilterOperator.StartsWithCaseSensitive, true)]
    [InlineData(FilterOperator.NotStartsWith, true)]
    [InlineData(FilterOperator.EndsWith, true)]
    [InlineData(FilterOperator.EndsWithCaseSensitive, true)]
    [InlineData(FilterOperator.NotEndsWith, true)]
    [InlineData(FilterOperator.Equal, false)]
    [InlineData(FilterOperator.NotEqual, false)]
    [InlineData(FilterOperator.GreaterThan, false)]
    [InlineData(FilterOperator.LessThanOrEqual, false)]
    public void IsStringOnly_ShouldClassifyCorrectly(FilterOperator op, bool expected)
    {
        FilterOperatorMap.IsStringOnly(op).Should().Be(expected);
    }

    #endregion

    #region Roundtrip Consistency

    [Fact(DisplayName = "Roundtrip: Every ToDslToken result is parseable via TryParse")]
    public void Roundtrip_AllTokensRoundtrip()
    {
        foreach (FilterOperator op in Enum.GetValues<FilterOperator>())
        {
            string token = FilterOperatorMap.ToDslToken(op);
            FilterOperatorMap.TryParse(token, out FilterOperator parsed).Should().BeTrue();
            parsed.Should().Be(op);
        }
    }

    #endregion
}
