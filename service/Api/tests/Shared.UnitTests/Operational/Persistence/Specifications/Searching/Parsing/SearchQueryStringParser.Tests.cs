using Shared.Operational.Persistence.Specifications.Searching;
using Shared.Operational.Persistence.Specifications.Searching.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchQueryStringParserTests
{
    [Fact(DisplayName = "Parse: Valid search term only returns success")]
    public void Parse_ValidSearchTermOnly_ShouldReturnSuccess()
    {
        Result<SearchModel> result = SearchQueryStringParser.Parse("hello", null, null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Term.Value.Should().Be("hello");
        result.Value.Fields.Should().BeEmpty();
        result.Value.Mode.Should().Be(SearchMode.Any);
    }

    [Theory(DisplayName = "Parse: Empty or null input returns Empty model")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyOrNull_ShouldReturnEmptyModel(String? search)
    {
        Result<SearchModel> result = SearchQueryStringParser.Parse(search, null, null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(SearchModel.Empty);
    }

    [Theory(DisplayName = "Parse: Mode parameter is handled correctly")]
    [InlineData("all", SearchMode.All)]
    [InlineData("any", SearchMode.Any)]
    [InlineData("unknown", SearchMode.Any)]
    [InlineData(null, SearchMode.Any)]
    public void Parse_Mode_ShouldHandleValues(String? modeStr, SearchMode expectedMode)
    {
        Result<SearchModel> result = SearchQueryStringParser.Parse("hello", null, modeStr, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Mode.Should().Be(expectedMode);
    }

    [Theory(DisplayName = "Parse: CaseSensitive parameter is handled correctly")]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("invalid", false)]
    [InlineData(null, false)]
    public void Parse_CaseSensitive_ShouldHandleValues(String? caseStr, Boolean expectedCaseSensitive)
    {
        Result<SearchModel> result = SearchQueryStringParser.Parse("Hello", null, null, caseStr);

        result.IsSuccess.Should().BeTrue();
        result.Value.Term.CaseSensitive.Should().Be(expectedCaseSensitive);
    }

    [Theory(DisplayName = "Parse: Fields are parsed correctly")]
    [InlineData("name,desc", 2, "name", "desc")]
    [InlineData(" name , desc ", 2, "name", "desc")]
    public void Parse_Fields_ShouldParseCorrectly(String fieldsStr, Int32 expectedCount, String expectedFirst, String expectedSecond)
    {
        Result<SearchModel> result = SearchQueryStringParser.Parse("hello", fieldsStr, null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Fields.Should().HaveCount(expectedCount);
        result.Value.Fields[0].Should().Be(expectedFirst);
        result.Value.Fields[1].Should().Be(expectedSecond);
    }

    [Fact(DisplayName = "Parse: Stores raw input string")]
    public void Parse_ShouldStoreRawInput()
    {
        Result<SearchModel> result = SearchQueryStringParser.Parse("hello world", "name,desc", "all", "true");

        result.IsSuccess.Should().BeTrue();
        result.Value.Term.Value.Should().Be("hello world");
        result.Value.RawInput.Should().NotBeNull();
    }
}