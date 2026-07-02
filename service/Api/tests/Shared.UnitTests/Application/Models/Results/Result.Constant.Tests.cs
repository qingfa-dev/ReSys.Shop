namespace Shared.UnitTests.Application.Models.Results;

public sealed class ResultConstantTests(ITestOutputHelper output)
{
    [Fact(DisplayName = "Constraints: MaxErrors should be 100")]
    public void Constraints_MaxErrors_ShouldBe100()
    {
        ResultConstant.Constraints.MaxErrors.Should().Be(100);
    }

    [Fact(DisplayName = "StatusCodes: all should have expected values")]
    public void StatusCodes_ShouldHaveExpectedValues()
    {
        ResultConstant.StatusCodes.Ok.Should().Be(200);
        ResultConstant.StatusCodes.Created.Should().Be(201);
        ResultConstant.StatusCodes.Accepted.Should().Be(202);
        ResultConstant.StatusCodes.NoContent.Should().Be(204);
        ResultConstant.StatusCodes.BadRequest.Should().Be(400);
        ResultConstant.StatusCodes.Unauthorized.Should().Be(401);
        ResultConstant.StatusCodes.Forbidden.Should().Be(403);
        ResultConstant.StatusCodes.NotFound.Should().Be(404);
        ResultConstant.StatusCodes.Conflict.Should().Be(409);
        ResultConstant.StatusCodes.Gone.Should().Be(410);
        ResultConstant.StatusCodes.UnprocessableEntity.Should().Be(422);
        ResultConstant.StatusCodes.TooManyRequests.Should().Be(429);
        ResultConstant.StatusCodes.InternalServerError.Should().Be(500);
        ResultConstant.StatusCodes.NotImplemented.Should().Be(501);
        ResultConstant.StatusCodes.BadGateway.Should().Be(502);
        ResultConstant.StatusCodes.ServiceUnavailable.Should().Be(503);
        ResultConstant.StatusCodes.GatewayTimeout.Should().Be(504);

        output.WriteLine("Verified {0} status code constants", 17);
    }

    [Fact(DisplayName = "DefaultValues: should have expected defaults")]
    public void DefaultValues_ShouldHaveExpectedDefaults()
    {
        ResultConstant.DefaultValues.IsSuccess.Should().BeTrue();
        ResultConstant.DefaultValues.StatusCode.Should().Be(ResultConstant.StatusCodes.Ok);
    }

    [Fact(DisplayName = "Messages: should have expected values")]
    public void Messages_ShouldHaveExpectedValues()
    {
        ResultConstant.Messages.Success.Should().Be("Operation completed successfully.");
        ResultConstant.Messages.Failure.Should().Be("Operation failed.");
    }
}
