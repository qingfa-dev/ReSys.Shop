using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortModelExtensionsFromJsonTests
{
    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData("   \t ")]
    public void FromJson_EmptyOrWhitespace_ShouldReturnEmpty(string? json)
    {
        Result<SortModel> result = SortModelExtensions.FromJson(json);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromJson_ValidArray_ShouldParse()
    {
        Result<SortModel> result = SortModelExtensions.FromJson("""[{"field":"Name"}]""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Clauses.Should().HaveCount(1);
        result.Value.Clauses[0].Field.Should().Be("Name");
    }

    [Fact]
    public void FromJson_InvalidJson_ShouldReturnFailure()
    {
        Result<SortModel> result = SortModelExtensions.FromJson("{invalid}");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void FromJson_NotAnArray_ShouldReturnFailure()
    {
        Result<SortModel> result = SortModelExtensions.FromJson("""{"field":"Name"}""");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void FromJson_WithAllowedFields_ShouldValidate()
    {
        Result<SortModel> result = SortModelExtensions.FromJson(
            """[{"field":"Name"}]""",
            ["Age"]);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
    }
}
