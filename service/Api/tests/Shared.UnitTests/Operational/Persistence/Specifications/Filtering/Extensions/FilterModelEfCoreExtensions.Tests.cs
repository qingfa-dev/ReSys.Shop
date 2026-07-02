using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Filtering.Extensions;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterModelEfCoreExtensionsTests
{
    private sealed class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    private static IQueryable<TestEntity> GetData() => new List<TestEntity>
    {
        new() { Name = "Apple", Age = 25, IsActive = true },
        new() { Name = "Banana", Age = 30, IsActive = false },
        new() { Name = "Orange", Age = 35, IsActive = true },
    }.AsQueryable();

    [Fact(DisplayName = "ApplyFilter(FilterModel): Valid model filters query")]
    public void ApplyFilter_ValidModel_ShouldFilter()
    {
        FilterGroup group = FilterGroup.FlatAnd(
            new FilterCondition[] { new("IsActive", FilterOperator.Equal, "true") });
        FilterModel model = new(group);
        IQueryable<TestEntity> query = GetData();

        List<TestEntity> result = query.ApplyFilter(model).ToList();

        result.Should().HaveCount(2);
    }

    [Fact(DisplayName = "ApplyFilter(FilterModel): Null model returns unchanged query")]
    public void ApplyFilter_NullModel_ShouldReturnUnchanged()
    {
        IQueryable<TestEntity> query = GetData();

        List<TestEntity> result = query.ApplyFilter(null).ToList();

        result.Should().HaveCount(3);
    }

    [Fact(DisplayName = "ApplyFilter(FilterModel): Empty model returns unchanged query")]
    public void ApplyFilter_EmptyModel_ShouldReturnUnchanged()
    {
        IQueryable<TestEntity> query = GetData();

        List<TestEntity> result = query.ApplyFilter(FilterModel.Empty).ToList();

        result.Should().HaveCount(3);
    }

    [Fact(DisplayName = "ApplyFilter(FilterModel): Invalid model with violations returns unchanged query")]
    public void ApplyFilter_InvalidModel_ShouldReturnUnchanged()
    {
        FilterGroup group = FilterGroup.FlatAnd(
            new FilterCondition[] { new("Forbidden", FilterOperator.Equal, "value") });
        HashSet<string> allowedFields = new(["Name", "Age"], StringComparer.OrdinalIgnoreCase);
        FilterModel model = new(group, allowedFields);
        IQueryable<TestEntity> query = GetData();

        List<TestEntity> result = query.ApplyFilter(model).ToList();

        result.Should().HaveCount(3);
    }

    [Fact(DisplayName = "ApplyFilter(Result): Success result filters query")]
    public void ApplyFilter_SuccessResult_ShouldFilter()
    {
        IQueryable<TestEntity> query = GetData();
        Result<FilterModel> modelResult = FilterModelExtensions.FromString("Name=Apple");

        List<TestEntity> result = query.ApplyFilter(modelResult).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "ApplyFilter(Result): Failure result returns unchanged query")]
    public void ApplyFilter_FailureResult_ShouldReturnUnchanged()
    {
        IQueryable<TestEntity> query = GetData();
        Result<FilterModel> modelResult = FilterModelExtensions.FromString("===invalid===");

        List<TestEntity> result = query.ApplyFilter(modelResult).ToList();

        result.Should().HaveCount(3);
    }

    [Fact(DisplayName = "ApplyFilterString: Parse and apply in one step")]
    public void ApplyFilterString_ShouldParseAndApply()
    {
        IQueryable<TestEntity> query = GetData();

        List<TestEntity> result = query.ApplyFilterString("Age>25").ToList();

        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().Contain(["Banana", "Orange"]);
    }

    [Fact(DisplayName = "ApplyFilterString: With allowedFields whitelist")]
    public void ApplyFilterString_WithAllowedFields_ShouldWork()
    {
        IQueryable<TestEntity> query = GetData();

        List<TestEntity> result = query.ApplyFilterString("Name=Apple", ["Name"]).ToList();

        result.Should().HaveCount(1);
    }

    [Fact(DisplayName = "ApplyFilterString: Invalid filter returns unchanged query")]
    public void ApplyFilterString_InvalidFilter_ShouldReturnUnchanged()
    {
        IQueryable<TestEntity> query = GetData();

        List<TestEntity> result = query.ApplyFilterString("===invalid===").ToList();

        result.Should().HaveCount(3);
    }

    [Fact(DisplayName = "ApplyFilterJson: Parse JSON and apply")]
    public void ApplyFilterJson_ShouldParseAndApply()
    {
        IQueryable<TestEntity> query = GetData();
        string json = """[{"field":"Name","op":"eq","value":"Apple"}]""";

        List<TestEntity> result = query.ApplyFilterJson(json).ToList();

        result.Should().HaveCount(1);
    }

    [Fact(DisplayName = "ApplyFilterQueryString: Parse triplets and apply")]
    public void ApplyFilterQueryString_ShouldParseAndApply()
    {
        IQueryable<TestEntity> query = GetData();

        List<TestEntity> result = query.ApplyFilterQueryString(["Age:gt:20", "IsActive:eq:true"]).ToList();

        result.Should().HaveCount(2);
    }

    [Fact(DisplayName = "ApplyFilter: Expression caching produces consistent results")]
    public void ApplyFilter_ExpressionCaching_ShouldBeConsistent()
    {
        IQueryable<TestEntity> query = GetData();
        Result<FilterModel> modelResult = FilterModelExtensions.FromString("Name=Apple");

        List<TestEntity> r1 = query.ApplyFilter(modelResult).ToList();
        List<TestEntity> r2 = query.ApplyFilter(modelResult).ToList();

        r1.Should().BeEquivalentTo(r2);
    }
}
