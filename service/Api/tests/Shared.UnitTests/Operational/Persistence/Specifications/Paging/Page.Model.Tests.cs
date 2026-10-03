using Shared.Operational.Persistence.Specifications.Paging;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Paging;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class PageModelTests
{
    [Fact]
    public void Empty_ShouldHaveDefaultValues()
    {
        PageModel empty = PageModel.Empty;

        empty.IsEmpty.Should().BeTrue();
        empty.Page.Should().Be(PageBounds.DefaultPageValue);
        empty.PageSize.Should().Be(PageBounds.DefaultPageSizeValue);
        empty.Skip.Should().Be(0);
    }

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        PageBounds bounds = new() { DefaultPage = 1, DefaultPageSize = 20, MaxPageSize = 200 };

        PageModel model = new(page: 3, pageSize: 20, bounds: bounds);

        model.Page.Should().Be(3);
        model.PageSize.Should().Be(20);
        model.Bounds.Should().BeSameAs(bounds);
        model.IsEmpty.Should().BeFalse();
        model.RawInput.Should().BeNull();
    }

    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 20, 40)]
    [InlineData(5, 100, 400)]
    public void Skip_ShouldCalculateCorrectOffset(int page, int pageSize, int expectedSkip)
    {
        PageBounds bounds = new() { DefaultPage = 1, DefaultPageSize = pageSize, MaxPageSize = 100 };

        PageModel model = new(page: page, pageSize: pageSize, bounds: bounds);

        model.Skip.Should().Be(expectedSkip);
    }

    [Theory]
    [InlineData(1, 10, 100, 10)]
    [InlineData(1, 10, 0, 0)]
    [InlineData(1, 10, 5, 1)]
    [InlineData(2, 25, 125, 5)]
    [InlineData(1, 10, 9, 1)]
    public void TotalPages_ShouldCalculateCorrectly(int page, int pageSize, long totalCount, int expectedPages)
    {
        PageBounds bounds = new() { DefaultPage = page, DefaultPageSize = pageSize, MaxPageSize = 100 };

        PageModel model = new(page: page, pageSize: pageSize, bounds: bounds);

        model.TotalPages(totalCount).Should().Be(expectedPages);
    }

    [Theory]
    [InlineData(1, 10, 100, true)]
    [InlineData(1, 10, 5, false)]
    [InlineData(1, 10, 10, false)]
    public void HasNextPage_ShouldReturnCorrectValue(int page, int pageSize, long totalCount, bool expected)
    {
        PageBounds bounds = new() { DefaultPage = page, DefaultPageSize = pageSize, MaxPageSize = 100 };

        PageModel model = new(page: page, pageSize: pageSize, bounds: bounds);

        model.HasNextPage(totalCount).Should().Be(expected);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(5, true)]
    public void HasPreviousPage_ShouldReturnTrueAfterFirstPage(int page, bool expected)
    {
        PageBounds bounds = PageBounds.Default;

        PageModel model = new(page: page, pageSize: 10, bounds: bounds);

        model.HasPreviousPage.Should().Be(expected);
    }

    [Fact]
    public void ToString_Empty_ShouldIndicateEmpty()
    {
        string output = PageModel.Empty.ToString();

        output.Should().Contain("empty");
    }

    [Fact]
    public void ToString_NonEmpty_ShouldContainPageAndSize()
    {
        PageBounds bounds = PageBounds.Default;

        PageModel model = new(page: 3, pageSize: 20, bounds: bounds);

        string output = model.ToString();

        output.Should().Contain("page=3");
        output.Should().Contain("size=20");
    }
}
