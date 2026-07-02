using Shared.Operational.Persistence.Specifications.Searching;
using Shared.Operational.Persistence.Specifications.Searching.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchTextParserTests
{
    [Theory(DisplayName = "Parse: Empty or null input returns SearchingModel.Empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   \t ")]
    public void Parse_EmptyOrNull_ShouldReturnEmpty(String? input)
    {
        SearchModel result = SearchTextParser.Parse(input);

        result.Should().BeSameAs(SearchModel.Empty);
    }

    [Theory(DisplayName = "Parse: Valid input returns model with correct properties")]
    [InlineData("hello", "hello")]
    [InlineData("  hello world  ", "hello world")]
    [InlineData("raw", "raw")]
    public void Parse_ValidInput_ShouldCreateModel(String input, String expectedTerm)
    {
        SearchModel result = SearchTextParser.Parse(input);

        result.Term.Value.Should().Be(expectedTerm);
        result.Fields.Should().BeEmpty();
        result.Mode.Should().Be(SearchMode.Any);
        result.IsEmpty.Should().BeFalse();
        result.RawInput.Should().Be(input);
    }
}