namespace Shared.UnitTests.Application.Models.Results;

public sealed class ValueResultToBaseTests
{
    [Fact(DisplayName = "ToBase: success Result<T> converts to success Result preserving properties")]
    public void ToBase_SuccessResult_ConvertsToSuccess()
    {
        var generic = Result<string>.Ok("hello", "completed", ("key", "value"));
        Result result = generic.ToBase();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Ok);
        result.Message.Should().Be("completed");
        result.Errors.Should().BeEmpty();
        result.Metadata.Should().ContainKey("key").WhoseValue.Should().Be("value");
    }

    [Fact(DisplayName = "ToBase: failure Result<T> converts to failure Result preserving errors")]
    public void ToBase_FailureResult_ConvertsToFailure()
    {
        List<Error> errors = [Error.NotFound("N.E", "missing")];
        var generic = Result<int>.NotFound("not found", errors: errors, ("trace", "abc"));
        Result result = generic.ToBase();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.NotFound);
        result.Message.Should().Be("not found");
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("N.E");
        result.Metadata.Should().ContainKey("trace").WhoseValue.Should().Be("abc");
    }

    [Fact(DisplayName = "ToBase: drops Value property")]
    public void ToBase_DropsValue()
    {
        var generic = Result<Guid>.Created(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        Result result = generic.ToBase();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Created);
    }

    [Fact(DisplayName = "ToBase: with null Value converts correctly")]
    public void ToBase_WithNullValue_ConvertsCorrectly()
    {
        var generic = Result<string?>.Ok(null);
        Result result = generic.ToBase();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Ok);
    }

    [Fact(DisplayName = "ToBase: preserves empty state")]
    public void ToBase_DefaultResult_ConvertsCorrectly()
    {
        var generic = Result<object>.Create();
        Result result = generic.ToBase();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Ok);
        result.Message.Should().BeNull();
        result.Errors.Should().BeEmpty();
        result.Metadata.Should().BeNull();
    }
}
