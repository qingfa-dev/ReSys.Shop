namespace Shared.UnitTests.Application.Models.Results;

public sealed class ResultToBaseTests
{
    [Fact(DisplayName = "ToBase: should return IResult with IsSuccess=false for non-empty errors")]
    public void ToBase_WithNonEmptyErrors_ReturnsFailure()
    {
        Error[] errors = [Error.Validation("V.E", "invalid")];
        IResultRecord result = Result.ToBase(errors);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.UnprocessableEntity);
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("V.E");
        result.Message.Should().Be("invalid");
        result.Metadata.Should().BeNull();
    }

    [Fact(DisplayName = "ToBase: should return IResult with success for empty errors")]
    public void ToBase_WithEmptyErrors_ReturnsDefault()
    {
        IResultRecord result = Result.ToBase([]);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "ToBase: returned IResult should be castable to Result")]
    public void ToBase_ShouldBeCastableToResult()
    {
        Error[] errors = [Error.BadRequest("B.E", "bad")];
        IResultRecord asInterface = Result.ToBase(errors);

        Result result = (Result)asInterface;

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
    }

    [Fact(DisplayName = "ToBase: with multiple errors preserves all errors")]
    public void ToBase_WithMultipleErrors_PreservesAll()
    {
        Error[] errors =
        [
            Error.Validation("V.E1", "first"),
            Error.Conflict("C.E2", "second")
        ];

        IResultRecord result = Result.ToBase(errors);

        result.Errors.Should().HaveCount(2);
        result.Errors[0].Code.Should().Be("V.E1");
        result.Errors[1].Code.Should().Be("C.E2");
    }
}
