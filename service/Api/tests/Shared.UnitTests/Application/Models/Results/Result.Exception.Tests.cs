namespace Shared.UnitTests.Application.Models.Results;

public sealed class ResultExceptionTests(ITestOutputHelper output)
{
    private static readonly Exception SampleException = new InvalidOperationException("test failure");

    public static IEnumerable<object[]> ExceptionFactoryData =>
    [
        [(Func<Exception, Result>)(ex => Result.BadRequest(ex)), ResultConstant.StatusCodes.BadRequest, nameof(Result.BadRequest)],
        [(Func<Exception, Result>)(ex => Result.Unauthorized(ex)), ResultConstant.StatusCodes.Unauthorized, nameof(Result.Unauthorized)],
        [(Func<Exception, Result>)(ex => Result.Forbidden(ex)), ResultConstant.StatusCodes.Forbidden, nameof(Result.Forbidden)],
        [(Func<Exception, Result>)(ex => Result.NotFound(ex)), ResultConstant.StatusCodes.NotFound, nameof(Result.NotFound)],
        [(Func<Exception, Result>)(ex => Result.Conflict(ex)), ResultConstant.StatusCodes.Conflict, nameof(Result.Conflict)],
        [(Func<Exception, Result>)(ex => Result.Validation(ex)), ResultConstant.StatusCodes.UnprocessableEntity, nameof(Result.Validation)],
        [(Func<Exception, Result>)(ex => Result.Unexpected(ex)), ResultConstant.StatusCodes.InternalServerError, nameof(Result.Unexpected)]
    ];

    [Theory(DisplayName = "Exception overload {2}: returns failure with correct status code and exception metadata")]
    [MemberData(nameof(ExceptionFactoryData))]
    public void ExceptionOverload_ReturnsFailureWithStatusCodeAndMetadata(
        Func<Exception, Result> factory,
        int expectedStatusCode,
        string factoryName)
    {
        var result = factory(SampleException);

        output.WriteLine("{0}: IsSuccess={1}, StatusCode={2}", factoryName, result.IsSuccess, result.StatusCode);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Metadata.Should().ContainKey("exception");
        var exceptionDict = result.Metadata!["exception"].Should().BeOfType<Dictionary<string, object?>>().Subject;
        exceptionDict["type"].Should().Be("System.InvalidOperationException");
        exceptionDict["message"].Should().Be("test failure");
    }

    [Fact(DisplayName = "BadRequest with exception and message: sets both")]
    public void BadRequest_WithExceptionAndMessage_SetsBoth()
    {
        var result = Result.BadRequest(SampleException, message: "bad request");

        result.Message.Should().Be("bad request");
        result.Metadata.Should().ContainKey("exception");
    }

    [Fact(DisplayName = "BadRequest with exception and errors: preserves errors")]
    public void BadRequest_WithExceptionAndErrors_PreservesErrors()
    {
        Error[] errors = [Error.Validation("V.E1", "error1")];

        var result = Result.BadRequest(SampleException, errors: errors);

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be("V.E1");
        result.Metadata.Should().ContainKey("exception");
    }

    [Fact(DisplayName = "Unexpected with exception and metadata: merges both")]
    public void Unexpected_WithExceptionAndMetadata_MergesBoth()
    {
        var result = Result.Unexpected(SampleException, metadata: ("traceId", "abc-123"));

        result.Metadata.Should().ContainKey("exception");
        result.Metadata.Should().ContainKey("traceId");
        result.Metadata!["traceId"].Should().Be("abc-123");
    }
}
