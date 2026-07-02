namespace Shared.UnitTests.Application.Models.Results;

public sealed class PagedResultMethodTests(ITestOutputHelper output)
{
    #region Success Factory

    [Fact(DisplayName = "Ok: should return success result with correct pagination")]
    public void Ok_ShouldReturnSuccessResult()
    {
        var items = new[] { "a", "b" };
        var result = PagedResult<string>.Ok(items, 1, 20, 42);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Ok);
        result.Items.Should().BeEquivalentTo(items);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(42);
        result.TotalPages.Should().Be(3);
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Ok: should accept message and metadata")]
    public void Ok_ShouldAcceptMessageAndMetadata()
    {
        var result = PagedResult<int>.Ok([], 1, 10, 0, "no results", ("query", "test"));

        result.Message.Should().Be("no results");
        result.Metadata.Should().ContainKey("query").WhoseValue.Should().Be("test");
    }

    [Fact(DisplayName = "Created: should return Created status code with correct pagination")]
    public void Created_ShouldReturnCreatedStatusCode()
    {
        var items = new[] { "x", "y" };
        var result = PagedResult<string>.Created(items, 1, 10, 25);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Created);
        result.Items.Should().BeEquivalentTo(items);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
    }

    [Fact(DisplayName = "Accepted: should return Accepted status code with correct pagination")]
    public void Accepted_ShouldReturnAcceptedStatusCode()
    {
        var items = new[] { "a" };
        var result = PagedResult<string>.Accepted(items, 2, 5, 8);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Accepted);
        result.Items.Should().BeEquivalentTo(items);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(8);
    }

    [Fact(DisplayName = "NoContent: should return NoContent status code with default pagination")]
    public void NoContent_ShouldReturnNoContentStatusCode()
    {
        var result = PagedResult<int>.NoContent();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.NoContent);
        result.Items.Should().BeEmpty();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    #endregion

    #region Failure Factories

    public static IEnumerable<object[]> FailureFactoryData =>
    [
        [(Func<PagedResult<object>>)(() => PagedResult<object>.Validation()), ResultConstant.StatusCodes.UnprocessableEntity, nameof(PagedResult<object>.Validation)],
        [(Func<PagedResult<object>>)(() => PagedResult<object>.NotFound()), ResultConstant.StatusCodes.NotFound, nameof(PagedResult<object>.NotFound)],
        [(Func<PagedResult<object>>)(() => PagedResult<object>.Conflict()), ResultConstant.StatusCodes.Conflict, nameof(PagedResult<object>.Conflict)],
        [(Func<PagedResult<object>>)(() => PagedResult<object>.Unexpected()), ResultConstant.StatusCodes.InternalServerError, nameof(PagedResult<object>.Unexpected)],
        [(Func<PagedResult<object>>)(() => PagedResult<object>.BadRequest()), ResultConstant.StatusCodes.BadRequest, nameof(PagedResult<object>.BadRequest)],
        [(Func<PagedResult<object>>)(() => PagedResult<object>.Unauthorized()), ResultConstant.StatusCodes.Unauthorized, nameof(PagedResult<object>.Unauthorized)],
        [(Func<PagedResult<object>>)(() => PagedResult<object>.Forbidden()), ResultConstant.StatusCodes.Forbidden, nameof(PagedResult<object>.Forbidden)]
    ];

    [Theory(DisplayName = "Failure factory {2}: should return IsSuccess=false with status code {1}")]
    [MemberData(nameof(FailureFactoryData))]
    public void FailureFactory_ShouldReturnCorrectStatusCode(
        Func<PagedResult<object>> factory,
        int expectedStatusCode,
        string factoryName)
    {
        PagedResult<object> result = factory();

        output.WriteLine("{0}: IsSuccess={1}, StatusCode={2}", factoryName, result.IsSuccess, result.StatusCode);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Items.Should().BeEmpty();
        result.Message.Should().BeNull();
    }

    [Fact(DisplayName = "Validation: with message and errors should propagate")]
    public void Validation_WithMessageAndErrors_ShouldPropagate()
    {
        Error[] errors = [Error.Validation("V.E", "validation error")];
        var result = PagedResult<object>.Validation("invalid", errors, ("field", "name"));

        result.Message.Should().Be("invalid");
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("V.E");
        result.Metadata.Should().ContainKey("field").WhoseValue.Should().Be("name");
    }

    [Fact(DisplayName = "BadRequest: with errors should include them")]
    public void BadRequest_WithErrors_ShouldIncludeErrors()
    {
        Error[] errors = [Error.BadRequest("V.E1", "E1"), Error.Validation("V.E2", "E2")];
        var result = PagedResult<object>.BadRequest(errors: errors);

        result.Errors.Should().HaveCount(2);
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Unauthorized: with errors should include them")]
    public void Unauthorized_WithErrors_ShouldIncludeErrors()
    {
        var error = Error.Unauthorized("Auth.E", "unauthorized");
        var result = PagedResult<object>.Unauthorized("not allowed", [error]);

        result.Message.Should().Be("not allowed");
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("Auth.E");
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Unauthorized);
    }

    [Fact(DisplayName = "Forbidden: with errors should include them")]
    public void Forbidden_WithErrors_ShouldIncludeErrors()
    {
        var error = Error.Forbidden("Auth.F", "forbidden");
        var result = PagedResult<object>.Forbidden(errors: [error], metadata: ("reason", "no access"));

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("Auth.F");
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Forbidden);
        result.Metadata.Should().ContainKey("reason").WhoseValue.Should().Be("no access");
    }

    #endregion

    #region Implicit Operators

    [Fact(DisplayName = "Implicit: Error -> PagedResult<T> should return failure")]
    public void Implicit_ErrorToPagedResult_ShouldReturnFailure()
    {
        PagedResult<string> result = Error.NotFound("R.Missing", "not found");

        output.WriteLine("Implicit Error->PagedResult: StatusCode={0}", result.StatusCode);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.NotFound);
        result.Message.Should().Be("not found");
        result.Errors.Should().ContainSingle();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Implicit: Error[] -> PagedResult<T> should return failure")]
    public void Implicit_ErrorArrayToPagedResult_ShouldReturnFailure()
    {
        PagedResult<int> result = new Error[]
        {
            Error.BadRequest("V.E1", "E1"),
            Error.Conflict("R.C", "conflict")
        };

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
        result.Errors.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Implicit: List<Error> -> PagedResult<T> should return failure")]
    public void Implicit_ErrorListToPagedResult_ShouldReturnFailure()
    {
        PagedResult<object> result = new List<Error> { Error.Unexpected("G.U", "unexpected") };

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.InternalServerError);
    }

    [Fact(DisplayName = "Implicit: empty Error[] -> PagedResult<T> should fallback to default status code")]
    public void Implicit_EmptyErrorArray_ShouldFallback()
    {
        PagedResult<string> result = Array.Empty<Error>();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(0);
    }

    [Fact(DisplayName = "Implicit: null List<Error> -> PagedResult<T> should fallback to InternalServerError")]
    public void Implicit_NullErrorList_ShouldFallbackToInternalServerError()
    {
        List<Error>? nullErrors = null;
        PagedResult<object> result = nullErrors!;

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.InternalServerError);
    }

    #endregion
}
