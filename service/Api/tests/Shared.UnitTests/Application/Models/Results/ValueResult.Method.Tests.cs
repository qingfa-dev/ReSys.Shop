namespace Shared.UnitTests.Application.Models.Results;

public sealed class ValueResultMethodTests(ITestOutputHelper output)
{
    #region Success Factories

    public static IEnumerable<object[]> SuccessFactoryData =>
    [
        [(Func<Result<string>>)(() => Result<string>.Ok("data")), ResultConstant.StatusCodes.Ok, nameof(Result.Ok)],
        [(Func<Result<Guid>>)(() => Result<Guid>.Created(Guid.NewGuid())), ResultConstant.StatusCodes.Created, nameof(Result.Created)],
        [(Func<Result<object?>>)(() => Result<object?>.Accepted(null)), ResultConstant.StatusCodes.Accepted, nameof(Result.Accepted)]
    ];

    [Theory(DisplayName = "Success factory {2}: should return IsSuccess=true with status code {1}")]
    [MemberData(nameof(SuccessFactoryData))]
    public void SuccessFactory_ShouldReturnCorrectStatusCode<T>(
        Func<Result<T>> factory,
        int expectedStatusCode,
        string factoryName)
    {
        Result<T> result = factory();

        output.WriteLine("{0}: IsSuccess={1}, StatusCode={2}", factoryName, result.IsSuccess, result.StatusCode);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Message.Should().BeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Ok: should accept message")]
    public void Ok_ShouldAcceptMessage()
    {
        var result = Result<int>.Ok(1, "item created");

        result.Message.Should().Be("item created");
    }

    [Fact(DisplayName = "Ok: should accept metadata")]
    public void Ok_ShouldAcceptMetadata()
    {
        var result = Result<string>.Ok("x", metadata: ("trace", "abc"));

        result.Metadata.Should().ContainKey("trace").WhoseValue.Should().Be("abc");
    }

    [Fact(DisplayName = "NoContent: should return NoContent status code with default value")]
    public void NoContent_ShouldReturnNoContentStatusCode()
    {
        var result = Result<int>.NoContent();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.NoContent);
        result.Value.Should().Be(default);
        result.Message.Should().BeNull();
    }

    [Fact(DisplayName = "NoContent: should accept message")]
    public void NoContent_ShouldAcceptMessage()
    {
        var result = Result<string>.NoContent("cache cleared");

        result.Message.Should().Be("cache cleared");
        result.Value.Should().Be(default(string));
    }

    #endregion

    #region Failure Factories

    public static IEnumerable<object[]> FailureFactoryData =>
    [
        [(Func<Result<object>>)(() => Result<object>.BadRequest()), ResultConstant.StatusCodes.BadRequest, nameof(Result.BadRequest)],
        [(Func<Result<object>>)(() => Result<object>.Unauthorized()), ResultConstant.StatusCodes.Unauthorized, nameof(Result.Unauthorized)],
        [(Func<Result<object>>)(() => Result<object>.Forbidden()), ResultConstant.StatusCodes.Forbidden, nameof(Result.Forbidden)],
        [(Func<Result<object>>)(() => Result<object>.NotFound()), ResultConstant.StatusCodes.NotFound, nameof(Result.NotFound)],
        [(Func<Result<object>>)(() => Result<object>.Conflict()), ResultConstant.StatusCodes.Conflict, nameof(Result.Conflict)],
        [(Func<Result<object>>)(() => Result<object>.Validation()), ResultConstant.StatusCodes.UnprocessableEntity, nameof(Result.Validation)],
        [(Func<Result<object>>)(() => Result<object>.Unexpected()), ResultConstant.StatusCodes.InternalServerError, nameof(Result.Unexpected)]
    ];

    [Theory(DisplayName = "Failure factory {2}: should return IsSuccess=false with status code {1}")]
    [MemberData(nameof(FailureFactoryData))]
    public void FailureFactory_ShouldReturnCorrectStatusCode(
        Func<Result<object>> factory,
        int expectedStatusCode,
        string factoryName)
    {
        Result<object> result = factory();

        output.WriteLine("{0}: IsSuccess={1}, StatusCode={2}", factoryName, result.IsSuccess, result.StatusCode);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Message.Should().BeNull();
    }

    [Fact(DisplayName = "BadRequest: with errors should include them")]
    public void BadRequest_WithErrors_ShouldIncludeErrors()
    {
        var errors = new List<Error> { Error.BadRequest("V.E1", "E1"), Error.Validation("V.E2", "E2") };
        var result = Result<object>.BadRequest(errors: errors);

        result.Errors.Should().HaveCount(2);
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
    }

    [Fact(DisplayName = "Failure factory: with metadata should include it")]
    public void FailureFactory_WithMetadata_ShouldIncludeMetadata()
    {
        var result = Result<object>.BadRequest(metadata: ("reason", "invalid"));

        result.Metadata.Should().ContainKey("reason").WhoseValue.Should().Be("invalid");
    }

    #endregion

    #region Implicit Operators

    [Fact(DisplayName = "Implicit: T -> Result<T> should return Ok")]
    public void Implicit_ValueToResult_ShouldReturnOk()
    {
        Result<string> result = "hello";

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Ok);
        result.Value.Should().Be("hello");
    }

    [Fact(DisplayName = "Implicit: int -> Result<int> should return Ok")]
    public void Implicit_IntValueToResult_ShouldReturnOk()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact(DisplayName = "Implicit: null -> Result<string?> should return Ok")]
    public void Implicit_NullValueToResult_ShouldReturnOk()
    {
        string? nullValue = null;
        Result<string?> result = nullValue;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact(DisplayName = "Implicit: Error -> Result<T> should return failure")]
    public void Implicit_ErrorToResult_ShouldReturnFailure()
    {
        Result<string> result = Error.NotFound("R.NotFound", "missing");

        output.WriteLine("Implicit Error->Result<T>: StatusCode={0}", result.StatusCode);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.NotFound);
        result.Message.Should().Be("missing");
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("R.NotFound");
    }

    [Fact(DisplayName = "Implicit: Error[] -> Result<T> should return failure")]
    public void Implicit_ErrorArrayToResult_ShouldReturnFailure()
    {
        Result<string> result = new Error[]
        {
            Error.BadRequest("V.E1", "E1"),
            Error.Conflict("R.C", "conflict")
        };

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
        result.Errors.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Implicit: List<Error> -> Result<T> should return failure")]
    public void Implicit_ErrorListToResult_ShouldReturnFailure()
    {
        Result<int> result = new List<Error> { Error.Unexpected("G.U", "unexpected") };

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.InternalServerError);
    }

    [Fact(DisplayName = "Implicit: empty Error[] -> Result<T> should fallback to default status code")]
    public void Implicit_EmptyErrorArray_ShouldFallback()
    {
        Result<string> result = Array.Empty<Error>();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(0);
    }

    [Fact(DisplayName = "Implicit: null List<Error> -> Result<T> should fallback to InternalServerError")]
    public void Implicit_NullErrorList_ShouldFallbackToInternalServerError()
    {
        List<Error>? nullErrors = null;
        Result<object> result = nullErrors!;

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.InternalServerError);
    }

    #endregion
}
