namespace Shared.UnitTests.Application.Models.Errors;

public sealed class ErrorExceptionTests
{
    [Fact(DisplayName = "FromException: populates Code, Message, Type, and exception metadata")]
    public void FromException_PopulatesPropertiesAndMetadata()
    {
        var ex = new InvalidOperationException("operation failed");

        var error = Error.FromException(ex, "Auth.Fail", "authentication failed");

        error.Code.Should().Be("Auth.Fail");
        error.Message.Should().Be("authentication failed");
        error.Type.Should().Be(ErrorType.Unexpected);
        error.Metadata.Should().ContainKey("exception");
        var exceptionDict = error.Metadata!["exception"].Should().BeOfType<Dictionary<string, object?>>().Subject;
        exceptionDict["type"].Should().Be("System.InvalidOperationException");
        exceptionDict["message"].Should().Be("operation failed");
    }

    [Fact(DisplayName = "FromException: accepts custom type parameter")]
    public void FromException_AcceptsCustomType()
    {
        var ex = new InvalidOperationException("validation error");

        var error = Error.FromException(ex, "V.Field", "field is invalid", ErrorType.Validation);

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("V.Field");
    }

    [Fact(DisplayName = "FromException: merges caller-supplied metadata with exception metadata")]
    public void FromException_MergesCallerMetadata()
    {
        var ex = new InvalidOperationException("fail");

        var error = Error.FromException(ex, "G.E", "error", ErrorType.Unexpected, ("Field", "email"), ("Resource", "User"));

        error.Metadata.Should().ContainKey("exception");
        error.Metadata.Should().ContainKey("Field");
        error.Metadata!["Field"].Should().Be("email");
        error.Metadata["Resource"].Should().Be("User");
    }

    [Fact(DisplayName = "FromException: nested exception produces nested dictionary")]
    public void FromException_NestedException()
    {
        var inner = new ArgumentNullException("param");
        var ex = new InvalidOperationException("outer", inner);

        var error = Error.FromException(ex, "G.Nested", "nested error");

        var exceptionDict = error.Metadata!["exception"].Should().BeOfType<Dictionary<string, object?>>().Subject;
        exceptionDict["type"].Should().Be("System.InvalidOperationException");
        var innerDict = exceptionDict["innerException"].Should().BeOfType<Dictionary<string, object?>>().Subject;
        innerDict["type"].Should().Be("System.ArgumentNullException");
    }
}
