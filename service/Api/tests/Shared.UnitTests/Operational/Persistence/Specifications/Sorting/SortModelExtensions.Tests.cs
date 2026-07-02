using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortModelExtensionsFromQueryStringTests
{
    [Fact]
    public void FromQueryString_EmptyArray_ShouldReturnEmpty()
    {
        Result<SortModel> result = SortModelExtensions.FromQueryString([]);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromQueryString_Null_ShouldReturnEmpty()
    {
        Result<SortModel> result = SortModelExtensions.FromQueryString(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromQueryString_ValidEntries_ShouldParse()
    {
        Result<SortModel> result = SortModelExtensions.FromQueryString(["Name:asc", "-CreatedAt"]);

        result.IsSuccess.Should().BeTrue();
        result.Value.Clauses.Should().HaveCount(2);
        result.Value.Clauses[0].Field.Should().Be("Name");
        result.Value.Clauses[1].Field.Should().Be("CreatedAt");
    }
}
