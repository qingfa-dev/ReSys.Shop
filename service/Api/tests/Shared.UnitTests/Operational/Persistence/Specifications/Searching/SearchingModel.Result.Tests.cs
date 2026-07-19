using Shared.Operational.Persistence.Specifications.Searching;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchingModelResultTests
{
    [Fact(DisplayName = "ToValidationResult: Valid model returns IsValid=true, empty violations")]
    public void ToValidationResult_ValidModel_ShouldReturnValid()
    {
        SearchModel model = new(new SearchTerm { Value = "hello" }, ["Name", "Description"]);

        SearchValidationResult result = model.ToValidationResult();

        result.IsValid.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact(DisplayName = "ToValidationResult: Invalid model returns IsValid=false with violations")]
    public void ToValidationResult_InvalidModel_ShouldReturnInvalid()
    {
        HashSet<string> allowedFields = new(["Name"], StringComparer.OrdinalIgnoreCase);
        SearchModel model = new(new SearchTerm { Value = "hello" }, ["Forbidden"], SearchMode.Any, allowedFields);

        SearchValidationResult result = model.ToValidationResult();

        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain("Forbidden");
    }

    [Fact(DisplayName = "ToValidationResult: AllowedFields exposed in result when present")]
    public void ToValidationResult_ShouldExposeAllowedFields()
    {
        HashSet<string> allowedFields = new(["Name"], StringComparer.OrdinalIgnoreCase);
        SearchModel model = new(new SearchTerm { Value = "hello" }, ["Name"], SearchMode.Any, allowedFields);

        SearchValidationResult result = model.ToValidationResult();

        result.AllowedFields.Should().Contain("Name");
    }

    [Fact(DisplayName = "ToValidationResult: AllowedFields is null in result when not set")]
    public void ToValidationResult_AllowedFieldsNull_WhenNotSet()
    {
        SearchModel model = new(new SearchTerm { Value = "hello" }, []);

        SearchValidationResult result = model.ToValidationResult();

        result.AllowedFields.Should().BeNull();
    }

    [Fact(DisplayName = "SearchValidationResult: Deconstruction works")]
    public void SearchValidationResult_Deconstruction_ShouldWork()
    {
        SearchValidationResult result = new() { IsValid = false, Violations = ["X"], AllowedFields = ["A"] };

        bool isValid = result.IsValid;
        IReadOnlyList<string> violations = result.Violations;
        IReadOnlyList<string>? allowedFields = result.AllowedFields;

        isValid.Should().BeFalse();
        violations.Should().Contain("X");
        allowedFields.Should().Contain("A");
    }

    [Theory(DisplayName = "SearchingModelResult.SuccessMsg: Should have expected values")]
    [InlineData("Parsed", "Search parsed successfully.")]
    [InlineData("Empty", "No search input provided; empty model returned.")]
    public void SuccessMsg_ShouldHaveExpectedValue(String name, String expectedMessage)
    {
        String actual = name switch
        {
            nameof(SearchingModelResult.SuccessMsg.Parsed) => SearchingModelResult.SuccessMsg.Parsed,
            nameof(SearchingModelResult.SuccessMsg.Empty) => SearchingModelResult.SuccessMsg.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown success message")
        };

        actual.Should().Be(expectedMessage);
    }

    [Fact(DisplayName = "SearchingModelResult.Failure: TermRequired has expected code")]
    public void Failure_TermRequired_ShouldHaveExpectedCode()
    {
        SearchingModelResult.Failure.TermRequired.Code.Should().Be("Search.Parsing.TermRequired");
    }

    [Theory(DisplayName = "SearchingModelResult.Failure: InvalidJson includes detail")]
    [InlineData("bad json")]
    [InlineData("unexpected token")]
    public void Failure_InvalidJson_ShouldIncludeDetail(string detail)
    {
        Error error = SearchingModelResult.Failure.InvalidJson(detail);

        error.Code.Should().Be("Search.Parsing.InvalidJson");
        error.Message.Should().Contain(detail);
    }

    [Theory(DisplayName = "SearchingModelResult.Failure: InvalidQueryString includes detail")]
    [InlineData("missing search term")]
    [InlineData("invalid format")]
    public void Failure_InvalidQueryString_ShouldIncludeDetail(string detail)
    {
        Error error = SearchingModelResult.Failure.InvalidQueryString(detail);

        error.Code.Should().Be("Search.Parsing.InvalidQueryString");
        error.Message.Should().Contain(detail);
    }
}