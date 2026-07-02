using Shared.Operational.Persistence.Specifications.Searching;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchingModelExtensionsTests
{
    [Theory(DisplayName = "FromText: Empty or null input returns SearchingModel.Empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   \t ")]
    public void FromText_EmptyOrNull_ShouldReturnEmpty(String? input)
    {
        SearchModel model = SearchModelExtensions.FromText(input!);

        model.Should().BeSameAs(SearchModel.Empty);
    }

    [Theory(DisplayName = "FromText: Valid input creates model with correct properties")]
    [InlineData(" hello ", "hello", " hello ")]
    [InlineData("hello", "hello", "hello")]
    public void FromText_ValidInput_ShouldCreateModel(String input, String expectedTerm, String expectedRawInput)
    {
        SearchModel model = SearchModelExtensions.FromText(input);

        model.IsEmpty.Should().BeFalse();
        model.Term.Value.Should().Be(expectedTerm);
        model.Fields.Should().BeEmpty();
        model.Mode.Should().Be(SearchMode.Any);
        model.RawInput.Should().Be(expectedRawInput);
    }

    [Theory(DisplayName = "FromJson: Empty or null returns Empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromJson_Empty_ShouldReturnEmpty(string? json)
    {
        Result<SearchModel> result = SearchModelExtensions.FromJson(json);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "FromJson: Valid JSON with term parses correctly")]
    public void FromJson_Valid_ShouldParse()
    {
        Result<SearchModel> result = SearchModelExtensions.FromJson(
            """{"term":"hello","fields":["Name","Description"],"mode":"all","caseSensitive":false}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Term.Value.Should().Be("hello");
        result.Value.Fields.Should().Contain(["Name", "Description"]);
        result.Value.Mode.Should().Be(SearchMode.All);
        result.Value.Term.CaseSensitive.Should().BeFalse();
    }

    [Fact(DisplayName = "FromJson: Missing term returns failure")]
    public void FromJson_MissingTerm_ShouldReturnFailure()
    {
        Result<SearchModel> result = SearchModelExtensions.FromJson("""{"fields":["Name"]}""");

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "FromJson: With allowedFields whitelist enforces validation")]
    public void FromJson_WithAllowedFields_ShouldValidate()
    {
        HashSet<string> allowed = new(StringComparer.OrdinalIgnoreCase) { "Name" };

        Result<SearchModel> result = SearchModelExtensions.FromJson(
            """{"term":"test","fields":["Name","Disallowed"]}""",
            allowed);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Violations.Should().Contain("Disallowed");
    }

    [Theory(DisplayName = "FromQueryString: Empty search returns Empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromQueryString_Empty_ShouldReturnEmpty(string? search)
    {
        Result<SearchModel> result = SearchModelExtensions.FromQueryString(search);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "FromQueryString: Valid search parses correctly")]
    public void FromQueryString_Valid_ShouldParse()
    {
        Result<SearchModel> result = SearchModelExtensions.FromQueryString(
            search: "hello",
            searchFields: "Name,Description",
            searchingMode: "all",
            caseSensitive: "true");

        result.IsSuccess.Should().BeTrue();
        result.Value.Term.Value.Should().Be("hello");
        result.Value.Term.CaseSensitive.Should().BeTrue();
        result.Value.Fields.Should().Contain(["Name", "Description"]);
        result.Value.Mode.Should().Be(SearchMode.All);
    }
}