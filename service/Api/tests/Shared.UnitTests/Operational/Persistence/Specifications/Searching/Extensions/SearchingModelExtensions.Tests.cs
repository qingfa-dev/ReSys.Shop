using Shared.Operational.Persistence.Specifications.Searching;
using Shared.Operational.Persistence.Specifications.Searching.Extensions;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchingModelQueryExtensionsTests
{
    private sealed class TestEntity
    {
        public String Name { get; set; } = String.Empty;
        public String Description { get; set; } = String.Empty;
        public String Category { get; set; } = String.Empty;
    }

    private static IQueryable<TestEntity> GetData() => new List<TestEntity>
    {
        new() { Name = "Apple iPhone", Description = "A smartphone", Category = "Electronics" },
        new() { Name = "Banana", Description = "A fruit", Category = "Groceries" },
        new() { Name = "Orange Juice", Description = "A drink", Category = "Beverages" },
        new() { Name = "Apricot Jam", Description = "A spread", Category = "Groceries" },
    }.AsQueryable();

    [Theory(DisplayName = "ApplySearch: Empty model or non-matching term returns expected count")]
    [InlineData(true, null, 4)]
    [InlineData(false, "zzznotfound", 0)]
    public void ApplySearch_EmptyOrNonMatching_ReturnsExpectedCount(Boolean isEmptyModel, String? term, Int32 expectedCount)
    {
        SearchModel model = isEmptyModel
            ? SearchModel.Empty
            : new(new SearchTerm(term!), ["Name"]);

        List<TestEntity> result = GetData()
            .ApplySearch(model, null)
            .ToList();

        result.Should().HaveCount(expectedCount);
    }

    [Theory(DisplayName = "ApplySearch: Any mode matching")]
    [InlineData("apple", "Name", 1)]
    [InlineData("groceries", "Name,Category", 2)]
    public void ApplySearch_AnyMode_Matching(String term, String fieldsStr, Int32 expectedCount)
    {
        String[] fields = fieldsStr.Split(',');
        SearchModel model = new(new SearchTerm(term), fields);

        List<TestEntity> result = GetData()
            .ApplySearch(model, null)
            .ToList();

        result.Should().HaveCount(expectedCount);
    }

    [Fact(DisplayName = "ApplySearch: All mode, matching all fields filters correctly")]
    public void ApplySearch_AllMode_MatchingAllFields_ShouldFilter()
    {
        SearchModel model = new(new SearchTerm("a"), ["Name", "Description"], SearchMode.All);

        List<TestEntity> result = GetData()
            .ApplySearch(model, null)
            .ToList();

        result.Should().HaveCount(4);
    }

    [Theory(DisplayName = "ApplySearch: Case sensitivity")]
    [InlineData("ELECTRONICS", false, "Category", 1)]
    [InlineData("Electronics", true, "Category", 1)]
    public void ApplySearch_CaseSensitivity(String term, Boolean caseSensitive, String fieldsStr, Int32 expectedCount)
    {
        String[] fields = fieldsStr.Split(',');
        SearchModel model = new(new SearchTerm(term, caseSensitive), fields);

        List<TestEntity> result = GetData()
            .ApplySearch(model, null)
            .ToList();

        result.Should().HaveCount(expectedCount);
    }

    [Fact(DisplayName = "ApplySearch: defaultFields fallback when model.Fields is empty")]
    public void ApplySearch_DefaultFieldsFallback_ShouldUseDefaults()
    {
        SearchModel model = new(new SearchTerm("fruit"), []);

        List<TestEntity> result = GetData()
            .ApplySearch(model, ["Description"])
            .ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Banana");
    }
}