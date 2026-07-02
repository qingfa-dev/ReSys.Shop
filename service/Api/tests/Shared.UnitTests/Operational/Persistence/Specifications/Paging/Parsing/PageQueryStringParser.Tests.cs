using Shared.Operational.Persistence.Specifications.Paging;
using Shared.Operational.Persistence.Specifications.Paging.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Paging.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class PageQueryStringParserTests
{
    [Fact]
    public void Parse_ValidStrings_ShouldReturnCorrectModel()
    {
        Result<PageModel> result = PageQueryStringParser.Parse("3", "20", PageBounds.Default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(3);
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public void Parse_BothNull_ShouldReturnEmpty()
    {
        Result<PageModel> result = PageQueryStringParser.Parse(null, null, PageBounds.Default);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Parse_OnlyPage_ShouldUseDefaultPageSize()
    {
        Result<PageModel> result = PageQueryStringParser.Parse("5", null, PageBounds.Default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(5);
        result.Value.PageSize.Should().Be(PageBounds.DefaultPageSizeValue);
    }

    [Fact]
    public void Parse_OnlyPageSize_ShouldUseDefaultPage()
    {
        Result<PageModel> result = PageQueryStringParser.Parse(null, "15", PageBounds.Default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(PageBounds.DefaultPageValue);
        result.Value.PageSize.Should().Be(15);
    }

    [Theory]
    [InlineData("abc", "10", "page")]
    [InlineData("1", "xyz", "pageSize")]
    public void Parse_NonInteger_ShouldReturnError(string pageStr, string pageSizeStr, string expectedProp)
    {
        Result<PageModel> result = PageQueryStringParser.Parse(pageStr, pageSizeStr, PageBounds.Default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "Paging.InvalidNumber" && e.Message.Contains(expectedProp));
    }

    [Fact]
    public void Parse_BothNonInteger_ShouldReturnMultipleErrors()
    {
        Result<PageModel> result = PageQueryStringParser.Parse("abc", "xyz", PageBounds.Default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}
