namespace Shared.UnitTests.Application.Models.Results;

public sealed class ValueResultTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact(DisplayName = "Create: should use defaults when no arguments")]
    public void Create_ShouldUseDefaults_WhenNoArguments()
    {
        var result = Result<string>.Create();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.Ok);
        result.Message.Should().BeNull();
        result.Errors.Should().BeEmpty();
        result.Metadata.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact(DisplayName = "Create: should set value when provided")]
    public void Create_ShouldSetValue_WhenValueProvided()
    {
        var result = Result<string>.Create(value: "hello");

        result.Value.Should().Be("hello");
    }

    [Fact(DisplayName = "Create: should set all properties when provided")]
    public void Create_ShouldSetProperties_WhenAllArgumentsProvided()
    {
        var errors = new List<Error> { Error.BadRequest("V.E", "error") };
        var result = Result<int>.Create(
            isSuccess: false,
            statusCode: ResultConstant.StatusCodes.BadRequest,
            value: 42,
            message: "fail",
            errors: errors,
            ("key", "value"));

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(ResultConstant.StatusCodes.BadRequest);
        result.Message.Should().Be("fail");
        result.Errors.Should().ContainSingle();
        result.Metadata.Should().ContainKey("key").WhoseValue.Should().Be("value");
    }
}
