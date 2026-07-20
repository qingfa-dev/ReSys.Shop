using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Filtering.Extensions;
using Shared.Operational.Persistence.Specifications.Paging;
using Shared.Operational.Persistence.Specifications.Paging.Extensions;
using Shared.Operational.Persistence.Specifications.Querying;
using Shared.Operational.Persistence.Specifications.Searching;
using Shared.Operational.Persistence.Specifications.Sorting;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Querying;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Querying")]
public sealed class QueryingModelApplyExtensionsTests : IDisposable
{
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Items => Set<TestEntity>();
    }

    private readonly TestDbContext _context;

    private static IReadOnlyList<string> SearchFields => ["Name", "Category"];

    private static IReadOnlyList<SortClause> DefaultSort => [new SortClause { Field = "Name", Direction = SortDirection.Ascending }];

    public QueryingModelApplyExtensionsTests()
    {
        DbContextOptionsBuilder<TestDbContext> optionsBuilder = new();
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _context = new TestDbContext(optionsBuilder.Options);

        List<TestEntity> seed =
        [
            new() { Name = "Apple", Age = 25, Category = "Fruit" },
            new() { Name = "Banana", Age = 30, Category = "Fruit" },
            new() { Name = "Carrot", Age = 20, Category = "Vegetable" },
            new() { Name = "Apricot", Age = 25, Category = "Fruit" },
        ];

        _context.Items.AddRange(seed);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private DbSet<TestEntity> GetData() => _context.Items;

    [Fact(DisplayName = "ApplyFilter: Empty filter leaves query unchanged")]
    public void ApplyFilter_EmptyFilter_ShouldReturnAll()
    {
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = SearchModel.Empty, Sort = SortModel.Empty, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplyFilter(model).ToList();

        result.Should().HaveCount(4);
    }

    [Fact(DisplayName = "ApplyFilter: Valid filter filters correctly")]
    public void ApplyFilter_ValidFilter_ShouldFilter()
    {
        FilterModel filter = FilterModelExtensions.FromString("Name=Apple").Value;
        QueryingModel model = new() { Filter = filter, Search = SearchModel.Empty, Sort = SortModel.Empty, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplyFilter(model).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "ApplySearch: Empty search leaves query unchanged")]
    public void ApplySearch_EmptySearch_ShouldReturnAll()
    {
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = SearchModel.Empty, Sort = SortModel.Empty, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplySearch(model, SearchFields).ToList();

        result.Should().HaveCount(4);
    }

    [Fact(DisplayName = "ApplySearch: Valid search finds matches")]
    public void ApplySearch_ValidSearch_ShouldFindMatches()
    {
        SearchModel search = new(new SearchTerm { Value = "fruit" }, []);
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = search, Sort = SortModel.Empty, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplySearch(model, SearchFields).ToList();

        result.Should().HaveCount(3);
    }

    [Fact(DisplayName = "ApplySort: Empty sort leaves query unchanged")]
    public void ApplySort_EmptySort_ShouldNotFail()
    {
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = SearchModel.Empty, Sort = SortModel.Empty, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplySort(model, DefaultSort).ToList();

        result.Should().HaveCount(4);
    }

    [Fact(DisplayName = "ApplySort: Valid sort sorts correctly")]
    public void ApplySort_ValidSort_ShouldSort()
    {
        SortModel sort = SortModelExtensions.FromString("Name desc").Value;
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = SearchModel.Empty, Sort = sort, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplySort(model).ToList();

        result[0].Name.Should().Be("Carrot");
    }

    [Fact(DisplayName = "ApplyQuerying: Empty model leaves query unchanged")]
    public void ApplyQuerying_EmptyModel_ShouldReturnAll()
    {
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = SearchModel.Empty, Sort = SortModel.Empty, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplyQuerying(model).ToList();

        result.Should().HaveCount(4);
    }

    [Fact(DisplayName = "ApplyQuerying: Filter only")]
    public void ApplyQuerying_FilterOnly_ShouldFilter()
    {
        FilterModel filter = FilterModelExtensions.FromString("Name=Apple").Value;
        QueryingModel model = new() { Filter = filter, Search = SearchModel.Empty, Sort = SortModel.Empty, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplyQuerying(model).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "ApplyQuerying: Search only")]
    public void ApplyQuerying_SearchOnly_ShouldFindMatches()
    {
        SearchModel search = new(new SearchTerm { Value = "fruit" }, []);
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = search, Sort = SortModel.Empty, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplyQuerying(model, SearchFields).ToList();

        result.Should().HaveCount(3);
    }

    [Fact(DisplayName = "ApplyQuerying: Sort only")]
    public void ApplyQuerying_SortOnly_ShouldSort()
    {
        SortModel sort = SortModelExtensions.FromString("Name desc").Value;
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = SearchModel.Empty, Sort = sort, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplyQuerying(model, defaultSortClauses: DefaultSort).ToList();

        result[0].Name.Should().Be("Carrot");
    }

    [Fact(DisplayName = "ApplyQuerying: All concerns chained together")]
    public void ApplyQuerying_AllConcerns_ShouldChain()
    {
        FilterModel filter = FilterModelExtensions.FromString("Category=Fruit").Value;
        SearchModel search = new(new SearchTerm { Value = "ap" }, []);
        SortModel sort = SortModelExtensions.FromString("Name desc").Value;
        QueryingModel model = new() { Filter = filter, Search = search, Sort = sort, Page = PageModel.Empty };

        List<TestEntity> result = GetData().ApplyQuerying(model, SearchFields, DefaultSort).ToList();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Apricot");
        result[1].Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "ToPagedResultAsync: Paginates correctly")]
    public async Task ToPagedResultAsync_ShouldPaginate()
    {
        SortModel sort = SortModelExtensions.FromString("Name asc").Value;
        PageModel page = PageModelExtensions.FromValues(1, 2).Value;
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = SearchModel.Empty, Sort = sort, Page = page };

        PagedResult<TestEntity> result = await GetData().ApplySort(model)
            .ToPagedResultAsync(model.Page);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(4);
        result.Items.ElementAt(0).Name.Should().Be("Apple");
        result.Items.ElementAt(1).Name.Should().Be("Apricot");
    }

    [Fact(DisplayName = "ToPagedOrAllAsync: Empty page returns all items")]
    public async Task ToPagedOrAllAsync_EmptyPage_ShouldReturnAll()
    {
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = SearchModel.Empty, Sort = SortModel.Empty, Page = PageModel.Empty };

        PagedResult<TestEntity> result = await GetData()
            .ApplyQuerying(model)
            .ToPagedOrAllAsync(model);

        result.Items.Should().HaveCount(4);
    }

    [Fact(DisplayName = "ToPagedOrEmptyAsync: Empty page returns no content")]
    public async Task ToPagedOrEmptyAsync_EmptyPage_ShouldReturnNoContent()
    {
        QueryingModel model = new() { Filter = FilterModel.Empty, Search = SearchModel.Empty, Sort = SortModel.Empty, Page = PageModel.Empty };

        PagedResult<TestEntity> result = await GetData()
            .ApplyQuerying(model)
            .ToPagedOrEmptyAsync(model);

        result.StatusCode.Should().Be(204);
        result.Items.Should().BeEmpty();
    }
}
