using Shared.Operational.Persistence.Specifications.Paging;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Paging;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class PageBoundsTests
{
    [Theory]
    [InlineData(nameof(PageBounds.DefaultPageValue), 1)]
    [InlineData(nameof(PageBounds.DefaultPageSizeValue), 10)]
    [InlineData(nameof(PageBounds.DefaultMaxPageSizeValue), 100)]
    public void Constants_ShouldHaveExpectedValues(string fieldName, int expected)
    {
        int actual = fieldName switch
        {
            nameof(PageBounds.DefaultPageValue) => PageBounds.DefaultPageValue,
            nameof(PageBounds.DefaultPageSizeValue) => PageBounds.DefaultPageSizeValue,
            nameof(PageBounds.DefaultMaxPageSizeValue) => PageBounds.DefaultMaxPageSizeValue,
            _ => -1
        };

        actual.Should().Be(expected);
    }

    [Fact]
    public void Default_ShouldUseDefaultConstants()
    {
        PageBounds @default = PageBounds.Default;

        @default.DefaultPage.Should().Be(PageBounds.DefaultPageValue);
        @default.DefaultPageSize.Should().Be(PageBounds.DefaultPageSizeValue);
        @default.MaxPageSize.Should().Be(PageBounds.DefaultMaxPageSizeValue);
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(100, 100)]
    public void NormalizePage_ShouldClampToAtLeastDefaultPage(int? raw, int expected)
    {
        int result = PageBounds.Default.NormalizePage(raw);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 10)]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 1)]
    [InlineData(10, 10)]
    [InlineData(50, 50)]
    [InlineData(100, 100)]
    [InlineData(200, 100)]
    [InlineData(1000, 100)]
    public void NormalizePageSize_ShouldClampToOneToMaxPageSize(int? raw, int expected)
    {
        int result = PageBounds.Default.NormalizePageSize(raw);

        result.Should().Be(expected);
    }

    [Fact]
    public void CustomBounds_ShouldApplyCorrectLimits()
    {
        PageBounds bounds = new(DefaultPage: 1, DefaultPageSize: 20, MaxPageSize: 200);

        bounds.NormalizePage(null).Should().Be(1);
        bounds.NormalizePageSize(null).Should().Be(20);
        bounds.NormalizePageSize(300).Should().Be(200);
    }
}
