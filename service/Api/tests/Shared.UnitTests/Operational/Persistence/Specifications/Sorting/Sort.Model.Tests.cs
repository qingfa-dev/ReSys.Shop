using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortModelTests
{
    [Fact]
    public void Empty_ShouldBeEmptyAndHaveNoClauses()
    {
        SortModel empty = SortModel.Empty;

        empty.IsEmpty.Should().BeTrue();
        empty.Clauses.Should().BeEmpty();
        empty.IsValid.Should().BeTrue();
    }

    [Fact]
    public void WithClauses_ShouldNotBeEmpty()
    {
        SortModel model = SortModelExtensions.FromString("Name asc").Value;

        model.IsEmpty.Should().BeFalse();
        model.Clauses.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("Name", new string[] { "Name" }, true)]
    [InlineData("Name", new string[] { "Age" }, false)]
    [InlineData("Name", null, true)]
    public void IsValid_ShouldRespectAllowedFields(string field, string[]? allowed, bool expectedValid)
    {
        SortModel model = SortModelExtensions.FromString(field, allowed).Value;

        model.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void Primary_WithClauses_ShouldReturnFirst()
    {
        SortModel model = SortModelExtensions.FromString("Name asc, CreatedAt desc").Value;

        model.Primary!.Field.Should().Be("Name");
    }

    [Fact]
    public void Primary_EmptyModel_ShouldReturnNull()
    {
        SortModel.Empty.Primary.Should().BeNull();
    }

    [Fact]
    public void RawInput_ShouldStoreOriginalInput()
    {
        string input = "Name asc";

        SortModel model = SortModelExtensions.FromString(input).Value;

        model.RawInput.Should().Be(input);
    }
}
