using Shared.Operational.Persistence.Specifications.Paging;
using Shared.Operational.Persistence.Specifications.Paging.Extensions;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Paging.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class PageModelInMemoryExtensionsTests
{
    private sealed record TestEntity
    {
        public int Id { get; init; }
        public string Name { get; init; } = default!;
    }

    [Fact]
    public void ToPagedResult_WithProjection_ShouldReturnCorrectPage()
    {
        List<TestEntity> source = Enumerable.Range(1, 30)
            .Select(i => new TestEntity { Id = i, Name = $"Item {i}" })
            .ToList();

        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 2, pageSize: 10, bounds: bounds);

        PagedResult<string> result = source.ToPagedResult(e => e.Name, page);

        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(30);
        result.Items.Should().HaveCount(10);
        result.Items.First().Should().Be("Item 11");
    }

    [Fact]
    public void ToPagedResult_WithoutProjection_ShouldReturnCorrectPage()
    {
        List<TestEntity> source = Enumerable.Range(1, 20)
            .Select(i => new TestEntity { Id = i, Name = $"Item {i}" })
            .ToList();

        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 1, pageSize: 5, bounds: bounds);

        PagedResult<TestEntity> result = source.ToPagedResult(page);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(20);
        result.Items.First().Id.Should().Be(1);
    }

    [Fact]
    public void ToPagedResult_EmptyList_ShouldReturnEmpty()
    {
        List<TestEntity> source = [];

        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 1, pageSize: 10, bounds: bounds);

        PagedResult<TestEntity> result = source.ToPagedResult(page);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void ToPagedResult_SingleItem_ShouldReturnThatItem()
    {
        List<TestEntity> source = [new TestEntity { Id = 1, Name = "Only" }];

        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 1, pageSize: 10, bounds: bounds);

        PagedResult<TestEntity> result = source.ToPagedResult(page);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public void ToPagedResult_PageBeyondTotal_ShouldReturnEmpty()
    {
        List<TestEntity> source = Enumerable.Range(1, 5)
            .Select(i => new TestEntity { Id = i, Name = $"Item {i}" })
            .ToList();

        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 10, pageSize: 10, bounds: bounds);

        PagedResult<TestEntity> result = source.ToPagedResult(page);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public void ToPagedResult_LastPartialPage_ShouldReturnRemainingItems()
    {
        List<TestEntity> source = Enumerable.Range(1, 25)
            .Select(i => new TestEntity { Id = i, Name = $"Item {i}" })
            .ToList();

        PageBounds bounds = PageBounds.Default;
        PageModel page = new(page: 3, pageSize: 10, bounds: bounds);

        PagedResult<TestEntity> result = source.ToPagedResult(page);

        result.Items.Should().HaveCount(5);
        result.Items.First().Id.Should().Be(21);
        result.Items.Last().Id.Should().Be(25);
    }
}
