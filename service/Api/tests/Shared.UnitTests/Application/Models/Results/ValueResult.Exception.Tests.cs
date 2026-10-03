namespace Shared.UnitTests.Application.Models.Results;

public sealed class ValueResultExceptionTests(ITestOutputHelper output)
{
    private static readonly Exception SampleException = new InvalidOperationException("test failure");

    public static IEnumerable<object[]> ExceptionFactoryData =>
    [
        [(Func<Exception, Result<string>>)(ex => Result<string>.BadRequest(ex)), ResultConstant.StatusCodes.BadRequest, nameof(Result<string>.BadRequest)],
        [(Func<Exception, Result<string>>)(ex => Result<string>.Unauthorized(ex)), ResultConstant.StatusCodes.Unauthorized, nameof(Result<string>.Unauthorized)],
        [(Func<Exception, Result<string>>)(ex => Result<string>.Forbidden(ex)), ResultConstant.StatusCodes.Forbidden, nameof(Result<string>.Forbidden)],
        [(Func<Exception, Result<string>>)(ex => Result<string>.NotFound(ex)), ResultConstant.StatusCodes.NotFound, nameof(Result<string>.NotFound)],
        [(Func<Exception, Result<string>>)(ex => Result<string>.Conflict(ex)), ResultConstant.StatusCodes.Conflict, nameof(Result<string>.Conflict)],
        [(Func<Exception, Result<string>>)(ex => Result<string>.Validation(ex)), ResultConstant.StatusCodes.UnprocessableEntity, nameof(Result<string>.Validation)],
        [(Func<Exception, Result<string>>)(ex => Result<string>.Unexpected(ex)), ResultConstant.StatusCodes.InternalServerError, nameof(Result<string>.Unexpected)]
    ];

    [Theory(DisplayName = "Exception overload {2}: returns failure with default value and exception metadata")]
    [MemberData(nameof(ExceptionFactoryData))]
    public void ExceptionOverload_ReturnsFailureWithDefaultValueAndMetadata(
        Func<Exception, Result<string>> factory,
        int expectedStatusCode,
        string factoryName)
    {
        var result = factory(SampleException);

        output.WriteLine("{0}: IsSuccess={1}, StatusCode={2}", factoryName, result.IsSuccess, result.StatusCode);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Value.Should().BeNull();
        result.Metadata.Should().ContainKey("exception");
    }

    [Fact(DisplayName = "NotFound with exception and message: sets both")]
    public void NotFound_WithExceptionAndMessage_SetsBoth()
    {
        var result = Result<string>.NotFound(SampleException, message: "not found");

        result.Message.Should().Be("not found");
        result.Metadata.Should().ContainKey("exception");
    }

    [Fact(DisplayName = "Validation with exception and errors: preserves errors")]
    public void Validation_WithExceptionAndErrors_PreservesErrors()
    {
        List<Error> errors = [Error.Validation("V.E1", "error1")];

        var result = Result<string>.Validation(SampleException, errors: errors);

        result.Errors.Should().HaveCount(1);
        result.Metadata.Should().ContainKey("exception");
    }

    [Fact(DisplayName = "Unexpected with exception and metadata: merges both")]
    public void Unexpected_WithExceptionAndMetadata_MergesBoth()
    {
        var result = Result<string>.Unexpected(SampleException, metadata: ("traceId", "abc-123"));

        result.Metadata.Should().ContainKey("exception");
        result.Metadata.Should().ContainKey("traceId");
    }
}
