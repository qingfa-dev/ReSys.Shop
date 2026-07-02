using Shared.Operational.Persistence.Specifications.Paging;
using Shared.Operational.Persistence.Specifications.Paging.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Paging.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class PageJsonParserTests
{
    [Fact]
    public void Parse_ValidObject_ShouldReturnCorrectModel()
    {
        Result<PageModel> result = PageJsonParser.Parse("""{"page":3,"pageSize":20}""", PageBounds.Default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(3);
        result.Value.PageSize.Should().Be(20);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_NullOrEmptyInput_ShouldReturnEmpty(string? json)
    {
        Result<PageModel> result = PageJsonParser.Parse(json, PageBounds.Default);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Parse_MalformedJson_ShouldReturnInvalidJsonError()
    {
        Result<PageModel> result = PageJsonParser.Parse("{bad}", PageBounds.Default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "Paging.InvalidJson");
    }

    [Fact]
    public void Parse_MissingProperties_ShouldUseDefaults()
    {
        Result<PageModel> result = PageJsonParser.Parse("{}", PageBounds.Default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(PageBounds.DefaultPageValue);
        result.Value.PageSize.Should().Be(PageBounds.DefaultPageSizeValue);
    }

    [Fact]
    public void Parse_ExtraProperties_ShouldBeIgnored()
    {
        Result<PageModel> result = PageJsonParser.Parse("""{"page":2,"extra":"ignored"}""", PageBounds.Default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(PageBounds.DefaultPageSizeValue);
    }
}
