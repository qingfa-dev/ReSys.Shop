namespace Shared.UnitTests.Application.Models.Results;

public sealed class ResultMethodTests(ITestOutputHelper output)
{
    #region Success Factories

    public static IEnumerable<object[]> SuccessFactoryData =>
    [
        [(Func<Result>)(() => Result.Ok()), ResultConstant.StatusCodes.Ok, nameof(Result.Ok)],
        [(Func<Result>)(() => Result.Created()), ResultConstant.StatusCodes.Created, nameof(Result.Created)],
        [(Func<Result>)(() => Result.Accepted()), ResultConstant.StatusCodes.Accepted, nameof(Result.Accepted)],
        [(Func<Result>)(() => Result.NoContent()), ResultConstant.StatusCodes.NoContent, nameof(Result.NoContent)]
    ];

    [Theory(DisplayName = "Success factory {2}: should return IsSuccess=true with status code {1}")]
    [MemberData(nameof(SuccessFactoryData))]
    public void SuccessFactory_ShouldReturnCorrectStatusCode(
        Func<Result> factory,
        int expectedStatusCode,
        string factoryName)
    {
        Result result = factory();

        output.WriteLine("{0}: IsSuccess={1}, StatusCode={2}", factoryName, result.IsSuccess, result.StatusCode);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Message.Should().BeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Ok: should set message")]
    public void Ok_ShouldSetMessage()
    {
        var result = Result.Ok("All good");

        result.Message.Should().Be("All good");
    }

    [Fact(DisplayName = "Ok: should accept metadata")]
    public void Ok_ShouldAcceptMetadata()
    {
        var result = Result.Ok(metadata: ("version", "1.0"));

        result.Metadata.Should().ContainKey("version").WhoseValue.Should().Be("1.0");
    }

    [Fact(DisplayName = "NoContent: should have no message")]
    public void NoContent_ShouldHaveNoMessage()
    {
        var result = Result.NoContent();

        result.Message.Should().BeNull();
    }

    #endregion

    #region Failure Factories

    public static IEnumerable<object[]> FailureFactoryData =>
    [
        [(Func<Result>)(() => Result.BadRequest()), ResultConstant.StatusCodes.BadRequest, nameof(Result.BadRequest)],
        [(Func<Result>)(() => Result.Unauthorized()), ResultConstant.StatusCodes.Unauthorized, nameof(Result.Unauthorized)],
        [(Func<Result>)(() => Result.Forbidden()), ResultConstant.StatusCodes.Forbidden, nameof(Result.Forbidden)],
        [(Func<Result>)(() => Result.NotFound()), ResultConstant.StatusCodes.NotFound, nameof(Result.NotFound)],
        [(Func<Result>)(() => Result.Conflict()), ResultConstant.StatusCodes.Conflict, nameof(Result.Conflict)],
        [(Func<Result>)(() => Result.Validation()), ResultConstant.StatusCodes.UnprocessableEntity, nameof(Result.Validation)],
        [(Func<Result>)(() => Result.Unexpected()), ResultConstant.StatusCodes.InternalServerError, nameof(Result.Unexpected)]
    ];

    [Theory(DisplayName = "Failure factory {2}: should return IsSuccess=false with status code {1}")]
    [MemberData(nameof(FailureFactoryData))]
    public void FailureFactory_ShouldReturnCorrectStatusCode(
        Func<Result> factory,
        int expectedStatusCode,
        string factoryName)
    {
        Result result = factory();

        output.WriteLine("{0}: IsSuccess={1}, StatusCode={2}", factoryName, result.IsSuccess, result.StatusCode);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Message.Should().BeNull();
    }

    [Fact(DisplayName = "BadRequest: with errors should include them")]
    public void BadRequest_WithErrors_ShouldIncludeErrors()
    {
        Error[] errors = [Error.BadRequest("Validation.E1", "E1"), Error.BadRequest("Validation.E2", "E2")];
        var result = Result.BadRequest(errors: errors);

        result.Errors.Should().HaveCount(2);
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
    }

    [Fact(DisplayName = "Failure factory: with metadata should include it")]
    public void FailureFactory_WithMetadata_ShouldIncludeMetadata()
    {
        var result = Result.BadRequest(metadata: ("reason", "invalid input"));

        result.Metadata.Should().ContainKey("reason").WhoseValue.Should().Be("invalid input");
    }

    #endregion

    #region Implicit Operators

    [Fact(DisplayName = "Implicit: Error -> Result should return failure")]
    public void Implicit_ErrorToResult_ShouldReturnFailure()
    {
        Result result = Error.NotFound("Resource.NotFound", "missing");

        output.WriteLine("Implicit Error->Result: StatusCode={0}, Errors={1}",
            result.StatusCode, result.Errors.Count);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.NotFound);
        result.Message.Should().Be("missing");
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("Resource.NotFound");
    }

    [Fact(DisplayName = "Implicit: Error[] -> Result should return failure")]
    public void Implicit_ErrorArrayToResult_ShouldReturnFailure()
    {
        Result result = new Error[]
        {
            Error.BadRequest("V.E1", "E1"),
            Error.Validation("V.E2", "E2")
        };

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
        result.Errors.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Implicit: List<Error> -> Result should return failure")]
    public void Implicit_ErrorListToResult_ShouldReturnFailure()
    {
        Result result = new List<Error> { Error.Conflict("R.C", "conflict") };

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Conflict);
        result.Errors.Should().ContainSingle();
    }

    [Fact(DisplayName = "Implicit: HashSet<Error> -> Result should return failure")]
    public void Implicit_ErrorHashSetToResult_ShouldReturnFailure()
    {
        Result result = new HashSet<Error> { Error.Unexpected("G.U", "unexpected") };

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.InternalServerError);
        result.Errors.Should().ContainSingle();
    }

    [Fact(DisplayName = "Implicit: empty Error[] -> Result should return failure")]
    public void Implicit_EmptyErrorArray_ShouldReturnFailure()
    {
        Result result = Array.Empty<Error>();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(0);
    }

    [Fact(DisplayName = "Implicit: empty List<Error> -> Result should return failure")]
    public void Implicit_EmptyErrorList_ShouldFallback()
    {
        Result result = new List<Error>();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(0);
    }

    [Fact(DisplayName = "Implicit: empty HashSet<Error> -> Result should return failure")]
    public void Implicit_EmptyHashSet_ShouldFallback()
    {
        Result result = new HashSet<Error>();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(0);
    }

    #endregion
}
