using Shared.Operational.Persistence.Specifications.Searching;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchingModelTests
{
    [Fact(DisplayName = "SearchingModel.Empty: IsEmpty is true, IsValid is true, Violations is empty")]
    public void Empty_ShouldBeEmptyAndValid()
    {
        SearchModel model = SearchModel.Empty;

        model.IsEmpty.Should().BeTrue();
        model.IsValid.Should().BeTrue();
        model.Violations.Should().BeEmpty();
    }

    [Fact(DisplayName = "SearchingModel: Non-empty model has IsEmpty false")]
    public void NonEmptyModel_ShouldHaveIsEmptyFalse()
    {
        SearchModel model = new(new SearchTerm { Value = "hello" }, []);

        model.IsEmpty.Should().BeFalse();
    }

    [Theory(DisplayName = "SearchingModel: IsValid respects AllowedFields whitelist")]
    [InlineData(true, "Name")]
    [InlineData(false, "Forbidden")]
    [InlineData(true, "AnyField")]
    public void IsValid_ShouldRespectWhitelist(Boolean expectedIsValid, String field)
    {
        HashSet<String>? allowedFields = expectedIsValid && field is "AnyField"
            ? null
            : new([field is "Forbidden" ? "Name" : field], StringComparer.OrdinalIgnoreCase);

        SearchModel model = new(new SearchTerm { Value = "hello" }, [field], SearchMode.Any, allowedFields);

        model.IsValid.Should().Be(expectedIsValid);
    }

    [Fact(DisplayName = "SearchingModel: Empty Fields with AllowedFields is valid")]
    public void EmptyFields_WithAllowedFields_ShouldBeValid()
    {
        HashSet<String> allowedFields = new(["Name"], StringComparer.OrdinalIgnoreCase);
        SearchModel model = new(new SearchTerm { Value = "hello" }, [], SearchMode.Any, allowedFields);

        model.IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "SearchingModel.HasField: Returns true for matching field, case-insensitive")]
    [InlineData("Name", true)]
    [InlineData("name", true)]
    [InlineData("NAME", true)]
    [InlineData("Description", false)]
    public void HasField_ShouldBeCaseInsensitive(String field, Boolean expected)
    {
        SearchModel model = new(new SearchTerm { Value = "hello" }, ["Name"]);

        model.HasField(field).Should().Be(expected);
    }

    [Fact(DisplayName = "SearchingModel: HasField returns false for empty Fields")]
    public void HasField_ShouldReturnFalse_ForEmptyFields()
    {
        SearchModel model = SearchModel.Empty;

        model.HasField("Name").Should().BeFalse();
    }

    [Theory(DisplayName = "SearchingModel: ResolveFields behavior")]
    [InlineData("Name,Description", "Category")]
    [InlineData("", "Name,Description")]
    public void ResolveFields_ShouldHandleEmptyAndNonEmpty(String fieldsStr, String defaultFieldsStr)
    {
        String[] fields = fieldsStr.Length > 0 ? fieldsStr.Split(',') : [];
        String[] defaultFields = defaultFieldsStr.Split(',');
        SearchModel model = new(new SearchTerm { Value = "hello" }, fields);

        IReadOnlyList<String> result = model.ResolveFields(defaultFields);

        result.Should().Equal(fields.Length > 0 ? fields : defaultFields);
    }

    [Theory(DisplayName = "SearchingModel: RawInput behavior")]
    [InlineData("hello", "hello")]
    [InlineData(null, null)]
    public void RawInput_ShouldReflectInput(String? rawInput, String? expected)
    {
        SearchModel model = new(new SearchTerm { Value = "hello" }, [], rawInput: rawInput);

        model.RawInput.Should().Be(expected);
    }
}