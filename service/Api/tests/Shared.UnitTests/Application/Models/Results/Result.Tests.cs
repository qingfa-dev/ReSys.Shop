namespace Shared.UnitTests.Application.Models.Results;

public sealed class ResultTests(ITestOutputHelper output)
{
    [Fact(DisplayName = "Create: should use defaults when no arguments")]
    public void Create_ShouldUseDefaults_WhenNoArguments()
    {
        var result = Result.Create();

        output.WriteLine("Default - IsSuccess={0}, StatusCode={1}", result.IsSuccess, result.StatusCode);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Ok);
        result.Message.Should().BeNull();
        result.Errors.Should().BeEmpty();
        result.Metadata.Should().BeNull();
    }

    [Fact(DisplayName = "Create: should set all properties when provided")]
    public void Create_ShouldSetProperties_WhenAllArgumentsProvided()
    {
        Error[] errors = [Error.BadRequest("Validation.Error", "error")];
        var result = Result.Create(
            isSuccess: false,
            statusCode: ResultConstant.StatusCodes.BadRequest,
            message: "fail",
            errors: errors,
            ("key", "value"));

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
        result.Message.Should().Be("fail");
        result.Errors.Should().HaveCount(1);
        result.Metadata.Should().ContainKey("key").WhoseValue.Should().Be("value");
    }

    [Fact(DisplayName = "Create: should accept null errors")]
    public void Create_ShouldAcceptNullErrors()
    {
        var result = Result.Create(errors: null);

        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Create: should accept empty metadata")]
    public void Create_ShouldAcceptEmptyMetadata()
    {
        var result = Result.Create(metadata: []);

        result.Metadata.Should().BeNull();
    }
}
