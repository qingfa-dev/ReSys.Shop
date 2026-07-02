namespace Shared.UnitTests.Application.Models.Results;

[Trait("Category", "Unit")]
[Trait("Module", "Results")]
[Trait("Feature", "EdgeCases")]
public sealed class ResultEdgeCaseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    #region Edge Cases

    [Fact(DisplayName = "Empty metadata should be null")]
    public void EmptyMetadata_ShouldBeNull()
    {
        var result = Result.Ok(metadata: []);

        result.Metadata.Should().BeNull();
    }

    [Fact(DisplayName = "Null message in Ok should be null")]
    public void NullMessage_ShouldBeNull()
    {
        var result = Result.Ok(message: null);

        result.Message.Should().BeNull();
    }

    [Fact(DisplayName = "IsFailure should be false for success")]
    public void IsFailure_ShouldBeFalse_ForSuccess()
    {
        var result = Result.Ok();

        result.IsFailure.Should().BeFalse();
    }

    [Fact(DisplayName = "IsFailure should be true for failure")]
    public void IsFailure_ShouldBeTrue_ForFailure()
    {
        var result = Result.BadRequest();

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Implicit chaining: Error -> Result should return failure")]
    public void ImplicitChaining_ErrorToResult_ShouldReturnFailure()
    {
        Result result = Error.NotFound("R.NotFound", "missing");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.NotFound);
        result.Message.Should().Be("missing");
    }

    [Fact(DisplayName = "Implicit chaining: empty Error[] -> Result should return failure")]
    public void ImplicitChaining_EmptyErrorArray_ShouldReturnFailure()
    {
        Result result = Array.Empty<Error>();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Implicit chaining: single Error[] -> Result should contain error")]
    public void ImplicitChaining_SingleErrorArray_ShouldContainError()
    {
        Result result = new[] { Error.Conflict("R.Conflict", "conflict") };

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("R.Conflict");
    }

    #endregion
}
