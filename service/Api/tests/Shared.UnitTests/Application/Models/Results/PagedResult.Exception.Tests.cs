namespace Shared.UnitTests.Application.Models.Results;

public sealed class PagedResultExceptionTests(ITestOutputHelper output)
{
    private static readonly Exception SampleException = new InvalidOperationException("test failure");

    public static IEnumerable<object[]> ExceptionFactoryData =>
    [
        [(Func<Exception, PagedResult<string>>)(ex => PagedResult<string>.BadRequest(ex)), ResultConstant.StatusCodes.BadRequest, nameof(PagedResult<string>.BadRequest)],
        [(Func<Exception, PagedResult<string>>)(ex => PagedResult<string>.Unauthorized(ex)), ResultConstant.StatusCodes.Unauthorized, nameof(PagedResult<string>.Unauthorized)],
        [(Func<Exception, PagedResult<string>>)(ex => PagedResult<string>.Forbidden(ex)), ResultConstant.StatusCodes.Forbidden, nameof(PagedResult<string>.Forbidden)],
        [(Func<Exception, PagedResult<string>>)(ex => PagedResult<string>.NotFound(ex)), ResultConstant.StatusCodes.NotFound, nameof(PagedResult<string>.NotFound)],
        [(Func<Exception, PagedResult<string>>)(ex => PagedResult<string>.Conflict(ex)), ResultConstant.StatusCodes.Conflict, nameof(PagedResult<string>.Conflict)],
        [(Func<Exception, PagedResult<string>>)(ex => PagedResult<string>.Validation(ex)), ResultConstant.StatusCodes.UnprocessableEntity, nameof(PagedResult<string>.Validation)],
        [(Func<Exception, PagedResult<string>>)(ex => PagedResult<string>.Unexpected(ex)), ResultConstant.StatusCodes.InternalServerError, nameof(PagedResult<string>.Unexpected)]
    ];

    [Theory(DisplayName = "Exception overload {2}: returns failure with default items and exception metadata")]
    [MemberData(nameof(ExceptionFactoryData))]
    public void ExceptionOverload_ReturnsFailureWithDefaultItemsAndMetadata(
        Func<Exception, PagedResult<string>> factory,
        int expectedStatusCode,
        string factoryName)
    {
        var result = factory(SampleException);

        output.WriteLine("{0}: IsSuccess={1}, StatusCode={2}", factoryName, result.IsSuccess, result.StatusCode);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Items.Should().BeEmpty();
        result.Metadata.Should().ContainKey("exception");
    }

    [Fact(DisplayName = "NotFound with exception and message: sets both")]
    public void NotFound_WithExceptionAndMessage_SetsBoth()
    {
        var result = PagedResult<string>.NotFound(SampleException, message: "not found");

        result.Message.Should().Be("not found");
        result.Metadata.Should().ContainKey("exception");
    }

    [Fact(DisplayName = "Validation with exception and errors: preserves errors")]
    public void Validation_WithExceptionAndErrors_PreservesErrors()
    {
        Error[] errors = [Error.Validation("V.E1", "error1")];

        var result = PagedResult<string>.Validation(SampleException, errors: errors);

        result.Errors.Should().HaveCount(1);
        result.Metadata.Should().ContainKey("exception");
    }

    [Fact(DisplayName = "Unexpected with exception and metadata: merges both")]
    public void Unexpected_WithExceptionAndMetadata_MergesBoth()
    {
        var result = PagedResult<string>.Unexpected(SampleException, metadata: ("traceId", "abc-123"));

        result.Metadata.Should().ContainKey("exception");
        result.Metadata.Should().ContainKey("traceId");
    }
}
