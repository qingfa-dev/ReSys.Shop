using Shared.Application.Models.Optionals;

namespace Shared.UnitTests.Application.Models.Results;

public sealed class ResultExtensionTests(ITestOutputHelper output)
{
    #region ToOptional
    [Fact(DisplayName = "ToOptional: on success Result should return Some with value")]
    public void ToOptional_OnSuccessResult_ShouldReturnSomeWithValue()
    {
        Result<string> result = Result<string>.Ok("hello");

        Optional<string> optional = result.ToOptional();

        output.WriteLine("ToOptional on success: HasValue={0}", optional.HasValue);

        optional.HasValue.Should().BeTrue();
    }

    [Fact(DisplayName = "ToOptional: on failure Result should return None")]
    public void ToOptional_OnFailureResult_ShouldReturnNone()
    {
        Error error = Error.BadRequest("E.Code", "failure");
        Result<string> result = Result<string>.BadRequest(errors: [error]);

        Optional<string> optional = result.ToOptional();

        optional.HasValue.Should().BeFalse();
    }

    [Fact(DisplayName = "ToOptional: value should match on success")]
    public void ToOptional_ValueShouldMatch_OnSuccess()
    {
        Result<int> result = Result<int>.Ok(42);

        Optional<int> optional = result.ToOptional();

        optional.Value.Should().Be(42);
    }
    #endregion
}
