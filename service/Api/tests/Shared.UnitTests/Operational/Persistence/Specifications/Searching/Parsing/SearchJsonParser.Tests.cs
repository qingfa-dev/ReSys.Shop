using System.Text.Json;

using Shared.Operational.Persistence.Specifications.Searching;
using Shared.Operational.Persistence.Specifications.Searching.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchJsonParserTests
{
    [Fact(DisplayName = "Parse: Valid JSON with term only returns success")]
    public void Parse_ValidJsonTermOnly_ShouldReturnSuccess()
    {
        String json = """{"term":"hello"}""";
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement element = doc.RootElement;

        Result<SearchModel> result = SearchJsonParser.Parse(element);

        result.IsSuccess.Should().BeTrue();
        result.Value.Term.Value.Should().Be("hello");
        result.Value.Fields.Should().BeEmpty();
        result.Value.Mode.Should().Be(SearchMode.Any);
    }

    [Fact(DisplayName = "Parse: Valid JSON with term, fields, mode returns success with All mode")]
    public void Parse_ValidJsonTermFieldsMode_ShouldReturnAllMode()
    {
        String json = """{"term":"hello","fields":["Name","Description"],"mode":"all"}""";
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement element = doc.RootElement;

        Result<SearchModel> result = SearchJsonParser.Parse(element);

        result.IsSuccess.Should().BeTrue();
        result.Value.Term.Value.Should().Be("hello");
        result.Value.Fields.Should().Equal(["Name", "Description"]);
        result.Value.Mode.Should().Be(SearchMode.All);
    }

    [Theory(DisplayName = "Parse: Invalid term JSON returns failure")]
    [InlineData(@"{""fields"":[""Name""]}")]
    [InlineData(@"{""term"":""""}")]
    [InlineData(@"{""term"":""   ""}")]
    public void Parse_InvalidTerm_ShouldReturnFailure(String json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement element = doc.RootElement;

        Result<SearchModel> result = SearchJsonParser.Parse(element);

        result.IsFailure.Should().BeTrue();
    }

    [Theory(DisplayName = "Parse: CaseSensitive parameter is parsed correctly")]
    [InlineData(@"{""term"":""Hello"",""caseSensitive"":true}", "Hello", true)]
    [InlineData(@"{""term"":""Hello"",""caseSensitive"":false}", "Hello", false)]
    public void Parse_CaseSensitive_ShouldParse(String json, String expectedTerm, Boolean expectedCaseSensitive)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement element = doc.RootElement;

        Result<SearchModel> result = SearchJsonParser.Parse(element);

        result.IsSuccess.Should().BeTrue();
        result.Value.Term.Value.Should().Be(expectedTerm);
        result.Value.Term.CaseSensitive.Should().Be(expectedCaseSensitive);
    }

    [Theory(DisplayName = "Parse: Mode parameter is parsed correctly")]
    [InlineData(@"{""term"":""hello"",""mode"":""any""}", SearchMode.Any)]
    [InlineData(@"{""term"":""hello"",""mode"":""unknown""}", SearchMode.Any)]
    [InlineData(@"{""term"":""hello"",""mode"":""all""}", SearchMode.All)]
    public void Parse_Mode_ShouldParse(String json, SearchMode expectedMode)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement element = doc.RootElement;

        Result<SearchModel> result = SearchJsonParser.Parse(element);

        result.IsSuccess.Should().BeTrue();
        result.Value.Mode.Should().Be(expectedMode);
    }
}