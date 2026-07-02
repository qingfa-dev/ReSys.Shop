using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortModelExtensionsFromStringTests
{
    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData("   \t ")]
    public void FromString_EmptyOrWhitespace_ShouldReturnEmpty(string? input)
    {
        Result<SortModel> result = SortModelExtensions.FromString(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromString_ValidSingleClause_ShouldParse()
    {
        Result<SortModel> result = SortModelExtensions.FromString("Name asc");

        result.IsSuccess.Should().BeTrue();
        result.Value.Clauses.Should().HaveCount(1);
        result.Value.Clauses[0].Field.Should().Be("Name");
    }

    [Fact]
    public void FromString_MultipleClauses_ShouldMaintainOrder()
    {
        Result<SortModel> result = SortModelExtensions.FromString("Name asc, CreatedAt desc");

        result.IsSuccess.Should().BeTrue();
        result.Value.Clauses.Should().HaveCount(2);
        result.Value.Clauses[0].Field.Should().Be("Name");
        result.Value.Clauses[1].Field.Should().Be("CreatedAt");
    }

    [Fact]
    public void FromString_InvalidSyntax_ShouldReturnFailure()
    {
        Result<SortModel> result = SortModelExtensions.FromString("Name sideways");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void FromString_DisallowedField_ShouldReturnModelWithViolation()
    {
        Result<SortModel> result = SortModelExtensions.FromString("Name", ["Age"]);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Violations.Should().Contain("Name");
    }
}
