using Shared.Operational.Persistence.Specifications.Paging;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Paging;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class PageModelExtensionsTests
{
    #region FromValues

    [Fact]
    public void FromValues_ValidInput_ShouldReturnCorrectModel()
    {
        Result<PageModel> result = PageModelExtensions.FromValues(page: 3, pageSize: 20);

        result.IsSuccess.Should().BeTrue();
        PageModel model = result.Value;
        model.Page.Should().Be(3);
        model.PageSize.Should().Be(20);
        model.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void FromValues_NullPage_UsesDefault()
    {
        Result<PageModel> result = PageModelExtensions.FromValues(page: null, pageSize: 20);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(PageBounds.DefaultPageValue);
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public void FromValues_NullPageSize_UsesDefault()
    {
        Result<PageModel> result = PageModelExtensions.FromValues(page: 3, pageSize: null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(3);
        result.Value.PageSize.Should().Be(PageBounds.DefaultPageSizeValue);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(-100, 1)]
    public void FromValues_InvalidPage_ClampsToDefault(int raw, int expected)
    {
        Result<PageModel> result = PageModelExtensions.FromValues(page: raw, pageSize: 10);

        result.Value.Page.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(150, 100)]
    [InlineData(200, 100)]
    public void FromValues_OutOfRangePageSize_ClampsCorrectly(int raw, int expected)
    {
        Result<PageModel> result = PageModelExtensions.FromValues(page: 1, pageSize: raw);

        result.Value.PageSize.Should().Be(expected);
    }

    [Fact]
    public void FromValues_BothNull_IsEmpty()
    {
        Result<PageModel> result = PageModelExtensions.FromValues(page: null, pageSize: null);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromValues_CustomBounds_AppliesCorrectly()
    {
        PageBounds bounds = new() { DefaultPage = 1, DefaultPageSize = 50, MaxPageSize = 500 };

        Result<PageModel> result = PageModelExtensions.FromValues(page: 2, pageSize: 100, bounds: bounds);

        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(100);
        result.Value.Bounds.Should().Be(bounds);
    }

    [Fact]
    public void FromValues_DefaultBounds_UsedWhenNull()
    {
        Result<PageModel> result = PageModelExtensions.FromValues(page: 1, pageSize: 10);

        result.Value.Bounds.Should().Be(PageBounds.Default);
    }

    #endregion FromValues

    #region FromQueryString

    [Fact]
    public void FromQueryString_ValidStrings_ShouldReturnCorrectModel()
    {
        Result<PageModel> result = PageModelExtensions.FromQueryString("3", "20");

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(3);
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public void FromQueryString_BothNull_ShouldReturnEmpty()
    {
        Result<PageModel> result = PageModelExtensions.FromQueryString(null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromQueryString_OnlyPage_ShouldUseDefaultPageSize()
    {
        Result<PageModel> result = PageModelExtensions.FromQueryString("2", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(PageBounds.DefaultPageSizeValue);
    }

    [Fact]
    public void FromQueryString_OnlyPageSize_ShouldUseDefaultPage()
    {
        Result<PageModel> result = PageModelExtensions.FromQueryString(null, "50");

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(PageBounds.DefaultPageValue);
        result.Value.PageSize.Should().Be(50);
    }

    [Theory]
    [InlineData("abc", "10", "page")]
    [InlineData("1", "xyz", "pageSize")]
    public void FromQueryString_NonInteger_ShouldReturnError(string pageStr, string pageSizeStr, string expectedProp)
    {
        Result<PageModel> result = PageModelExtensions.FromQueryString(pageStr, pageSizeStr);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "Paging.InvalidNumber" && e.Message.Contains(expectedProp));
    }

    [Fact]
    public void FromQueryString_EmptyStrings_ShouldReturnEmpty()
    {
        Result<PageModel> result = PageModelExtensions.FromQueryString("", "");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromQueryString_CustomBounds_AppliesCorrectly()
    {
        PageBounds bounds = new() { DefaultPage = 1, DefaultPageSize = 50, MaxPageSize = 500 };

        Result<PageModel> result = PageModelExtensions.FromQueryString("5", "200", bounds);

        result.Value.Bounds.Should().Be(bounds);
        result.Value.Page.Should().Be(5);
        result.Value.PageSize.Should().Be(200);
    }

    #endregion FromQueryString

    #region FromJson

    [Fact]
    public void FromJson_ValidObject_ShouldReturnCorrectModel()
    {
        Result<PageModel> result = PageModelExtensions.FromJson("""{"page":3,"pageSize":20}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(3);
        result.Value.PageSize.Should().Be(20);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromJson_NullOrEmptyInput_ShouldReturnEmpty(string? json)
    {
        Result<PageModel> result = PageModelExtensions.FromJson(json);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromJson_Malformed_ShouldReturnInvalidJsonError()
    {
        Result<PageModel> result = PageModelExtensions.FromJson("not json");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "Paging.InvalidJson");
    }

    [Fact]
    public void FromJson_MissingProperties_ShouldUseDefaults()
    {
        Result<PageModel> result = PageModelExtensions.FromJson("""{}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(PageBounds.DefaultPageValue);
        result.Value.PageSize.Should().Be(PageBounds.DefaultPageSizeValue);
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void FromJson_ExtraProperties_ShouldBeIgnored()
    {
        Result<PageModel> result = PageModelExtensions.FromJson("""{"page":4,"pageSize":15,"extra":"ignored"}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(4);
        result.Value.PageSize.Should().Be(15);
    }

    [Fact]
    public void FromJson_CustomBounds_AppliesCorrectly()
    {
        PageBounds bounds = new() { DefaultPage = 1, DefaultPageSize = 30, MaxPageSize = 300 };

        Result<PageModel> result = PageModelExtensions.FromJson("""{"page":3,"pageSize":30}""", bounds);

        result.Value.Bounds.Should().Be(bounds);
    }

    #endregion FromJson
}
