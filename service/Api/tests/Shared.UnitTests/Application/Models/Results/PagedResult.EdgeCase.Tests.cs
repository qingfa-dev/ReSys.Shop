namespace Shared.UnitTests.Application.Models.Results;

[Trait("Category", "Unit")]
[Trait("Module", "Results")]
[Trait("Feature", "EdgeCases")]
public sealed class PagedResultEdgeCaseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    #region TotalPages

    [Fact(DisplayName = "TotalPages: pageSize zero should return 0")]
    public void TotalPages_PageSizeZero_ShouldReturnZero()
    {
        var result = PagedResult<int>.Create(totalCount: 100, pageSize: 0);

        result.TotalPages.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "TotalPages: negative pageSize should return 0")]
    public void TotalPages_NegativePageSize_ShouldReturnZero()
    {
        var result = PagedResult<int>.Create(totalCount: 100, pageSize: -5);

        result.TotalPages.Should().Be(0);
    }

    [Fact(DisplayName = "TotalPages: negative totalCount should produce negative pages")]
    public void TotalPages_NegativeTotalCount_ShouldProduceNegativePages()
    {
        var result = PagedResult<int>.Create(totalCount: -10, pageSize: 10);

        result.TotalPages.Should().Be(-1);
    }

    [Fact(DisplayName = "TotalPages: zero totalCount should return 0")]
    public void TotalPages_ZeroTotalCount_ShouldReturnZero()
    {
        var result = PagedResult<int>.Create(totalCount: 0, pageSize: 10);

        result.TotalPages.Should().Be(0);
    }

    [Fact(DisplayName = "TotalPages: exact division should return correct count")]
    public void TotalPages_ExactDivision_ShouldReturnCorrect()
    {
        var result = PagedResult<int>.Create(totalCount: 100, pageSize: 10);

        result.TotalPages.Should().Be(10);
    }

    [Fact(DisplayName = "TotalPages: remainder should round up")]
    public void TotalPages_Remainder_ShouldRoundUp()
    {
        var result = PagedResult<int>.Create(totalCount: 101, pageSize: 10);

        result.TotalPages.Should().Be(11);
    }

    #endregion

    #region Empty Items

    [Fact(DisplayName = "Empty items collection should be empty")]
    public void EmptyItems_ShouldBeEmpty()
    {
        var result = PagedResult<int>.Create(items: []);

        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Null items should default to empty collection")]
    public void NullItems_ShouldDefaultToEmpty()
    {
        var result = PagedResult<int>.Create(items: null);

        result.Items.Should().BeEmpty();
    }

    #endregion

    #region Page Edge Cases

    [Fact(DisplayName = "Page zero should be allowed")]
    public void PageZero_ShouldBeAllowed()
    {
        var result = PagedResult<int>.Create(page: 0);

        result.PageNumber.Should().Be(0);
    }

    [Fact(DisplayName = "Negative page should be allowed")]
    public void NegativePage_ShouldBeAllowed()
    {
        var result = PagedResult<int>.Create(page: -1);

        result.PageNumber.Should().Be(-1);
    }

    #endregion

    #region Metadata

    [Fact(DisplayName = "Empty metadata should be null")]
    public void EmptyMetadata_ShouldBeNull()
    {
        var result = PagedResult<int>.Create(metadata: []);

        result.Metadata.Should().BeNull();
    }

    [Fact(DisplayName = "Metadata when not provided should be null")]
    public void Metadata_WhenNotProvided_ShouldBeNull()
    {
        var result = PagedResult<int>.Create();

        result.Metadata.Should().BeNull();
    }

    #endregion
}
