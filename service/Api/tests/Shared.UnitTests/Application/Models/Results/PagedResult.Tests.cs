namespace Shared.UnitTests.Application.Models.Results;

public sealed class PagedResultTests(ITestOutputHelper output)
{
    [Fact(DisplayName = "Create: should use defaults when no arguments")]
    public void Create_ShouldUseDefaults_WhenNoArguments()
    {
        var result = PagedResult<object>.Create();

        output.WriteLine("Default - Page={0}, PageSize={1}, TotalCount={2}, TotalPages={3}",
            result.PageNumber, result.PageSize, result.TotalCount, result.TotalPages);

        result.Items.Should().BeEmpty();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Ok);
        result.Message.Should().BeNull();
        result.Errors.Should().BeEmpty();
        result.Metadata.Should().BeNull();
    }

    [Fact(DisplayName = "Create: should set pagination properties")]
    public void Create_ShouldSetPaginationProperties()
    {
        var items = new[] { 1, 2, 3 };
        var result = PagedResult<int>.Create(
            items: items,
            page: 2,
            pageSize: 5,
            totalCount: 20);

        result.Items.Should().BeEquivalentTo(items);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(20);
    }

    [Fact(DisplayName = "Create: should set IsSuccess false when errors provided")]
    public void Create_ShouldSetIsSuccessFalse_WhenErrorsProvided()
    {
        Error[] errors = [Error.BadRequest("V.E", "error")];
        var result = PagedResult<object>.Create(
            errors: errors,
            isSuccess: false,
            statusCode: ResultConstant.StatusCodes.BadRequest);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
        result.Errors.Should().ContainSingle();
        result.Items.Should().BeEmpty();
    }

    #region TotalPages

    public static IEnumerable<object[]> TotalPagesData =>
    [
        [100, 10, 10, "even division"],
        [101, 10, 11, "round up remainder"],
        [100, 0, 0, "pageSize zero"],
        [0, 10, 0, "totalCount zero"],
        [5, 1, 5, "pageSize one"],
        [10, 10, 1, "pageSize equals totalCount"],
        [5, 10, 1, "pageSize exceeds totalCount"]
    ];

    [Theory(DisplayName = "TotalPages: {3} — {0} items / {1} per page = {2} pages")]
    [MemberData(nameof(TotalPagesData))]
    public void TotalPages_ShouldComputeCorrectly(
        long totalCount,
        int pageSize,
        int expectedPages,
        string scenario)
    {
        var result = PagedResult<int>.Create(totalCount: totalCount, pageSize: pageSize);

        output.WriteLine("{0}: {1} / {2} => {3} (expected {4})",
            scenario, totalCount, pageSize, result.TotalPages, expectedPages);

        result.TotalPages.Should().Be(expectedPages);
    }

    #endregion
}
