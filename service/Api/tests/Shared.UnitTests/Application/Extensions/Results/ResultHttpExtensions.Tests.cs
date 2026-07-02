using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.UnitTests.Application.Extensions.Results;

file sealed class NonClosingMemoryStream : MemoryStream
{
    // CA2215: base.Dispose intentionally not called — stream must stay open
    // after TypedResults disposes it, so tests can read the response body.
#pragma warning disable CA2215
    protected override void Dispose(bool disposing)
    {
    }
#pragma warning restore CA2215
}

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Results.Http")]
public sealed class ResultHttpExtensionsTests(ITestOutputHelper output)
{
    private static DefaultHttpContext CreateHttpContext()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddRouting();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return new DefaultHttpContext { RequestServices = serviceProvider, Response = { Body = new NonClosingMemoryStream() } };
    }

    private static string ReadBody(DefaultHttpContext ctx)
    {
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(ctx.Response.Body);
        return reader.ReadToEnd();
    }

    private static void AssertContentType(DefaultHttpContext ctx)
    {
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    private static void AssertBodyContains(DefaultHttpContext ctx, string expected)
    {
        string body = ReadBody(ctx);
        body.Should().Contain(expected);
    }

    private static void AssertBodyNotContains(DefaultHttpContext ctx, string expected)
    {
        string body = ReadBody(ctx);
        body.Should().NotContain(expected);
    }

    private static void AssertBodyEmpty(DefaultHttpContext ctx)
    {
        string body = ReadBody(ctx);
        body.Should().BeEmpty();
    }

    #region ToResult - Success Status Codes

    [Theory(DisplayName = "ToResult success with common status codes returns correct status code")]
    [InlineData(StatusCodes.Status200OK)]
    [InlineData(StatusCodes.Status201Created)]
    [InlineData(StatusCodes.Status202Accepted)]
    [InlineData(StatusCodes.Status204NoContent)]
    public async Task ToResult_SuccessWithCommonStatusCodes_ReturnsCorrectStatusCode(int statusCode)
    {
        Result result = statusCode switch
        {
            StatusCodes.Status200OK => Result.Ok(),
            StatusCodes.Status201Created => Result.Created(),
            StatusCodes.Status202Accepted => Result.Accepted(),
            StatusCodes.Status204NoContent => Result.NoContent(),
            _ => throw new ArgumentOutOfRangeException(nameof(statusCode))
        };
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        output.WriteLine("Expected: {0}, Actual: {1}", statusCode, httpContext.Response.StatusCode);
        httpContext.Response.StatusCode.Should().Be(statusCode);

        if (statusCode == StatusCodes.Status204NoContent)
        {
            AssertBodyEmpty(httpContext);
        }
        else
        {
            AssertContentType(httpContext);
            AssertBodyContains(httpContext, "\"isSuccess\":true");
            AssertBodyContains(httpContext, "\"statusCode\":" + statusCode);
            AssertBodyNotContains(httpContext, "\"value\"");
        }
    }

    [Fact(DisplayName = "ToResult success with default returns Ok")]
    public async Task ToResult_SuccessWithDefault_ReturnsOk()
    {
        Result result = Result.Ok();
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"isSuccess\":true");
        AssertBodyNotContains(httpContext, "\"value\"");
    }

    #endregion

    #region ToResult - Failure Status Codes

    [Theory(DisplayName = "ToResult failure with status code returns correct status code")]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status401Unauthorized)]
    [InlineData(StatusCodes.Status404NotFound)]
    [InlineData(StatusCodes.Status405MethodNotAllowed)]
    [InlineData(StatusCodes.Status406NotAcceptable)]
    [InlineData(StatusCodes.Status408RequestTimeout)]
    [InlineData(StatusCodes.Status409Conflict)]
    [InlineData(StatusCodes.Status412PreconditionFailed)]
    [InlineData(StatusCodes.Status413PayloadTooLarge)]
    [InlineData(StatusCodes.Status415UnsupportedMediaType)]
    [InlineData(StatusCodes.Status422UnprocessableEntity)]
    [InlineData(StatusCodes.Status429TooManyRequests)]
    [InlineData(StatusCodes.Status500InternalServerError)]
    [InlineData(StatusCodes.Status502BadGateway)]
    [InlineData(StatusCodes.Status503ServiceUnavailable)]
    [InlineData(StatusCodes.Status504GatewayTimeout)]
    public async Task ToResult_FailureWithStatusCode_ReturnsCorrectStatusCode(int failureStatusCode)
    {
        Error error = Error.Create("Error.Code", "Error description", failureStatusCode);
        Result result = error;
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        output.WriteLine("Expected: {0}, Actual: {1}", failureStatusCode, httpContext.Response.StatusCode);
        httpContext.Response.StatusCode.Should().Be(failureStatusCode);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"isSuccess\":false");
        AssertBodyContains(httpContext, "\"statusCode\":" + failureStatusCode);
        AssertBodyContains(httpContext, "\"errors\"");
        AssertBodyNotContains(httpContext, "\"value\"");
    }

    [Fact(DisplayName = "ToResult failure with default status returns BadRequest")]
    public async Task ToResult_FailureWithDefaultStatus_ReturnsBadRequest()
    {
        Error error = Error.BadRequest("Error.Code", "Error description");
        Result result = error;
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"isSuccess\":false");
        AssertBodyContains(httpContext, "\"statusCode\":400");
        AssertBodyContains(httpContext, "\"errors\"");
        AssertBodyNotContains(httpContext, "\"value\"");
    }

    [Fact(DisplayName = "ToResult failure with Forbidden should return 403")]
    public async Task ToResult_FailureWithForbidden_ReturnsForbidden()
    {
        Error error = Error.Forbidden("Auth.Denied", "Forbidden");
        Result result = error;
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"isSuccess\":false");
        AssertBodyContains(httpContext, "\"statusCode\":403");
        AssertBodyContains(httpContext, "\"errors\"");
        AssertBodyNotContains(httpContext, "\"value\"");
    }

    [Theory(DisplayName = "ToResult failure with custom status code returns custom status code")]
    [InlineData(418)]
    [InlineData(451)]
    [InlineData(599)]
    public async Task ToResult_FailureWithCustomStatusCode_ReturnsCustomStatusCode(int customStatusCode)
    {
        Error error = Error.Create("Error.Code", "Error description", customStatusCode);
        Result result = error;
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        output.WriteLine("Expected: {0}, Actual: {1}", customStatusCode, httpContext.Response.StatusCode);
        httpContext.Response.StatusCode.Should().Be(customStatusCode);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"isSuccess\":false");
        AssertBodyContains(httpContext, "\"errors\"");
        AssertBodyNotContains(httpContext, "\"value\"");
    }

    [Fact(DisplayName = "ToResult success with custom status code returns Ok")]
    public async Task ToResult_SuccessWithCustomStatusCode_ReturnsOk()
    {
        Result result = Result.Create(isSuccess: true, statusCode: 250);
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"isSuccess\":true");
        AssertBodyNotContains(httpContext, "\"value\"");
    }

    #endregion

    #region ToResult<T> - Generic Version

    [Fact(DisplayName = "ToResult<T> success returns Ok with value")]
    public async Task ToResultT_Success_ReturnsOkWithValue()
    {
        Result<string> result = Result<string>.Ok("test-value");
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"value\":\"test-value\"");
        AssertBodyContains(httpContext, "\"isSuccess\":true");
    }

    [Fact(DisplayName = "ToResult<T> success with Created returns Created with value")]
    public async Task ToResultT_SuccessWithCreated_ReturnsCreatedWithValue()
    {
        Result<string> result = Result<string>.Created("test-value");
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"value\":\"test-value\"");
    }

    [Fact(DisplayName = "ToResult<T> success with Accepted returns Accepted")]
    public async Task ToResultT_SuccessWithAccepted_ReturnsAccepted()
    {
        Result<string> result = Result<string>.Accepted("test-value");
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"value\":\"test-value\"");
    }

    [Fact(DisplayName = "ToResult<T> success with NoContent returns NoContent")]
    public async Task ToResultT_SuccessWithNoContent_ReturnsNoContent()
    {
        Result<string> result = Result<string>.NoContent();
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        AssertBodyEmpty(httpContext);
    }

    [Fact(DisplayName = "ToResult<T> success with custom status code returns Ok")]
    public async Task ToResultT_SuccessWithCustomStatusCode_ReturnsOk()
    {
        Result<string> result = Result<string>.Create(
            isSuccess: true,
            statusCode: 250,
            value: "value");
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"value\":\"value\"");
    }

    #endregion

    #region ToPagedResult

    [Fact(DisplayName = "ToPagedResult success returns Ok")]
    public async Task ToPagedResult_Success_ReturnsOk()
    {
        PagedResult<string> pagedResult = PagedResult<string>.Ok(
            new List<string> { "item" }, 1, 10, 1);
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = pagedResult.ToPagedResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"items\"");
        AssertBodyContains(httpContext, "\"page\":1");
        AssertBodyContains(httpContext, "\"pageSize\":10");
        AssertBodyContains(httpContext, "\"totalCount\":1");
    }

    [Fact(DisplayName = "ToPagedResult success with Created returns Created")]
    public async Task ToPagedResult_SuccessWithCreated_ReturnsCreated()
    {
        PagedResult<string> pagedResult = PagedResult<string>.Created(
            new List<string> { "item" }, 1, 10, 1);
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = pagedResult.ToPagedResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"items\"");
        AssertBodyContains(httpContext, "\"page\":1");
        AssertBodyContains(httpContext, "\"pageSize\":10");
        AssertBodyContains(httpContext, "\"totalCount\":1");
    }

    [Fact(DisplayName = "ToPagedResult success with Accepted returns Accepted")]
    public async Task ToPagedResult_SuccessWithAccepted_ReturnsAccepted()
    {
        PagedResult<string> pagedResult = PagedResult<string>.Accepted(
            new List<string> { "item" }, 1, 10, 1);
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = pagedResult.ToPagedResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"items\"");
        AssertBodyContains(httpContext, "\"page\":1");
        AssertBodyContains(httpContext, "\"pageSize\":10");
        AssertBodyContains(httpContext, "\"totalCount\":1");
    }

    [Fact(DisplayName = "ToPagedResult success with NoContent returns NoContent")]
    public async Task ToPagedResult_SuccessWithNoContent_ReturnsNoContent()
    {
        PagedResult<string> pagedResult = PagedResult<string>.NoContent();
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = pagedResult.ToPagedResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        AssertBodyEmpty(httpContext);
    }

    [Fact(DisplayName = "ToPagedResult success with custom status code returns Ok")]
    public async Task ToPagedResult_SuccessWithCustomStatusCode_ReturnsOk()
    {
        PagedResult<string> pagedResult = PagedResult<string>.Create(
            items: new List<string> { "item" },
            page: 1,
            pageSize: 10,
            totalCount: 1,
            isSuccess: true,
            statusCode: 250);
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = pagedResult.ToPagedResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"items\"");
        AssertBodyContains(httpContext, "\"page\":1");
        AssertBodyContains(httpContext, "\"pageSize\":10");
        AssertBodyContains(httpContext, "\"totalCount\":1");
    }

    #endregion

    #region ToCreatedResult

    [Fact(DisplayName = "ToCreatedResult success returns Created with location")]
    public async Task ToCreatedResult_Success_ReturnsCreatedWithLocation()
    {
        Result<string> result = Result<string>.Created("created-value");
        string locationUri = "/api/resources/123";
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToCreatedResult(locationUri);

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        httpContext.Response.Headers.Location.ToString().Should().Be(locationUri);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"value\":\"created-value\"");
    }

    #endregion

    #region ToAcceptedResult

    [Theory(DisplayName = "ToAcceptedResult success returns Accepted")]
    [InlineData(null)]
    [InlineData("/api/resources/123")]
    public async Task ToAcceptedResult_Success_ReturnsAccepted(string? locationUri)
    {
        Result result = Result.Accepted();
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToAcceptedResult(locationUri);

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        if (locationUri != null)
        {
            httpContext.Response.Headers.Location.ToString().Should().Be(locationUri);
            AssertContentType(httpContext);
            AssertBodyContains(httpContext, "\"isSuccess\":true");
        }
        else
        {
            // Accepted without location still returns JSON with result body
            AssertContentType(httpContext);
            AssertBodyContains(httpContext, "\"isSuccess\":true");
        }
    }

    [Fact(DisplayName = "ToAcceptedResult<T> success returns Accepted with value")]
    public async Task ToAcceptedResultT_Success_ReturnsAcceptedWithValue()
    {
        Result<string> result = Result<string>.Accepted("accepted-value");
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToAcceptedResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"value\":\"accepted-value\"");
    }

    [Fact(DisplayName = "ToAcceptedResult failure returns failure status")]
    public async Task ToAcceptedResult_Failure_ReturnsFailureStatus()
    {
        Error error = Error.Conflict("Resource.Conflict", "Conflict");
        Result result = error;
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToAcceptedResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"isSuccess\":false");
        AssertBodyContains(httpContext, "\"errors\"");
    }

    #endregion

    #region ToNoContentResult

    [Fact(DisplayName = "ToNoContentResult success returns NoContent")]
    public async Task ToNoContentResult_Success_ReturnsNoContent()
    {
        Result result = Result.NoContent();
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToNoContentResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        AssertBodyEmpty(httpContext);
    }

    [Fact(DisplayName = "ToNoContentResult<T> success returns NoContent")]
    public async Task ToNoContentResultT_Success_ReturnsNoContent()
    {
        Result<string> result = Result<string>.NoContent();
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToNoContentResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        AssertBodyEmpty(httpContext);
    }

    [Fact(DisplayName = "ToNoContentResult failure returns failure status")]
    public async Task ToNoContentResult_Failure_ReturnsFailureStatus()
    {
        Error error = Error.Validation("Validation.Error", "Validation failed");
        Result result = error;
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToNoContentResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"isSuccess\":false");
        AssertBodyContains(httpContext, "\"errors\"");
    }

    #endregion

    #region Edge Cases

    [Fact(DisplayName = "ToResult<T> generic failure should downcast to plain Result")]
    public async Task ToResult_GenericFailure_ShouldDowncastToPlainResult()
    {
        Error error = Error.BadRequest("Error.Code", "Error Description");
        Result<string> result = error;
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();
        await httpResult.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        AssertContentType(httpContext);
        AssertBodyNotContains(httpContext, "\"value\"");
    }

    [Fact(DisplayName = "ToResult success with message returns Ok with message")]
    public async Task ToResult_SuccessWithMessage_ReturnsOkWithMessage()
    {
        Result result = Result.Ok("Operation completed successfully");
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"message\":\"Operation completed successfully\"");
        AssertBodyContains(httpContext, "\"isSuccess\":true");
    }

    [Fact(DisplayName = "ToResult failure with multiple failures returns first failure status")]
    public async Task ToResult_FailureWithMultipleFailures_ReturnsFirstErrorStatus()
    {
        List<Error> failures =
        [
            Error.BadRequest("Error.First", "First error"),
            Error.Conflict("Error.Second", "Second error")
        ];
        Result result = failures;
        DefaultHttpContext httpContext = CreateHttpContext();

        var httpResult = result.ToResult();

        await httpResult.ExecuteAsync(httpContext);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        AssertContentType(httpContext);
        AssertBodyContains(httpContext, "\"errors\"");
        AssertBodyContains(httpContext, "\"Error.First\"");
        AssertBodyContains(httpContext, "\"Error.Second\"");
    }

    #endregion
}
