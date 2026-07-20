using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Specifications.Paging;
using Shared.Operational.Persistence.Specifications.Paging.Extensions;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Paging.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class PageModelEfCoreExtensionsTests : IDisposable
{
    private sealed record TestEntity
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    private sealed record TestDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = default!;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Items => Set<TestEntity>();
    }

    private readonly TestDbContext _context;

    public PageModelEfCoreExtensionsTests()
    {
        DbContextOptionsBuilder<TestDbContext> optionsBuilder = new();
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _context = new TestDbContext(optionsBuilder.Options);

        List<TestEntity> seed = Enumerable.Range(1, 25)
            .Select(i => new TestEntity { Id = i, Name = $"Item {i}" })
            .ToList();

        _context.Items.AddRange(seed);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task ToPagedResultAsync_WithProjection_ShouldReturnCorrectPage()
    {
        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 2, pageSize: 5, bounds: bounds);

        PagedResult<TestDto> result = await _context.Items
            .ToPagedResultAsync(e => new TestDto { Id = e.Id, Name = e.Name }, page);

        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.Items.First().Id.Should().Be(6);
    }

    [Fact]
    public async Task ToPagedResultAsync_WithoutProjection_ShouldReturnCorrectPage()
    {
        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 1, pageSize: 10, bounds: bounds);

        PagedResult<TestEntity> result = await _context.Items.ToPagedResultAsync(page);

        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ToPagedOrAllAsync_EmptyModel_ShouldReturnAllItems()
    {
        PageModel page = PageModel.Empty;

        PagedResult<TestDto> result = await _context.Items
            .ToPagedOrAllAsync(e => new TestDto { Id = e.Id, Name = e.Name }, page);

        result.Items.Should().HaveCount(25);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task ToPagedOrAllAsync_NonEmptyModel_ShouldPaginate()
    {
        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 2, pageSize: 3, bounds: bounds);

        PagedResult<TestDto> result = await _context.Items
            .ToPagedOrAllAsync(e => new TestDto { Id = e.Id, Name = e.Name }, page);

        result.Items.Should().HaveCount(3);
        result.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task ToPagedOrEmptyAsync_EmptyModel_ShouldReturnNoContent()
    {
        PageModel page = PageModel.Empty;

        PagedResult<TestDto> result = await _context.Items
            .ToPagedOrEmptyAsync(e => new TestDto { Id = e.Id, Name = e.Name }, page);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task ToPagedOrEmptyAsync_NonEmptyModel_ShouldPaginate()
    {
        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 3, pageSize: 5, bounds: bounds);

        PagedResult<TestEntity> result = await _context.Items.ToPagedOrEmptyAsync(page);

        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(3);
    }

    [Fact]
    public async Task ToPagedResultAsync_PageBeyondTotal_ShouldReturnEmpty()
    {
        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 100, pageSize: 10, bounds: bounds);

        PagedResult<TestEntity> result = await _context.Items.ToPagedResultAsync(page);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ToPagedResultAsync_WithoutProjection_OrAllEmpty_ShouldReturnAll()
    {
        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 1, pageSize: 50, bounds: bounds);

        PagedResult<TestEntity> result = await _context.Items.ToPagedResultAsync(page);

        result.Items.Should().HaveCount(25);
    }
}
