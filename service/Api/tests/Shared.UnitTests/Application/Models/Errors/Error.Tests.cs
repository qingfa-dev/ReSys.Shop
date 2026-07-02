namespace Shared.UnitTests.Application.Models.Errors;

public sealed class ErrorTests(ITestOutputHelper output)
{
    [Fact(DisplayName = "Create: should set properties when all parameters provided")]
    public void Create_ShouldSetProperties_WhenAllParametersProvided()
    {
        var error = Error.Create(
            "Auth.InvalidToken",
            "The provided token is invalid.",
            ErrorType.Unauthorized,
            ("Field", "token"));

        output.WriteLine("Created error: {0}", error.Code);

        error.Code.Should().Be("Auth.InvalidToken");
        error.Message.Should().Be("The provided token is invalid.");
        error.Type.Should().Be(ErrorType.Unauthorized);
        error.Metadata.Should().ContainKey("Field").WhoseValue.Should().Be("token");
    }

    [Fact(DisplayName = "Create: should use default ErrorType when not specified")]
    public void Create_ShouldUseDefaultType_WhenNotSpecified()
    {
        var error = Error.Create("General.Unexpected", "Something went wrong.");

        output.WriteLine("Default type: {0}", error.Type);

        error.Type.Should().Be(ErrorConstant.DefaultValues.Type);
    }

    [Fact(DisplayName = "Create: should set Metadata to null when no metadata provided")]
    public void Create_ShouldSetMetadataToNull_WhenNoMetadataProvided()
    {
        var error = Error.Create("General.Unexpected", "Something went wrong.");

        error.Metadata.Should().BeNull();
    }

    [Fact(DisplayName = "Create: should accept multiple metadata entries")]
    public void Create_ShouldAcceptMultipleMetadataEntries()
    {
        var error = Error.Create(
            "Validation.FieldRequired",
            "Field is required.",
            ErrorType.Validation,
            ("Field", "email"),
            ("Resource", "User"),
            ("AttemptedValue", null));

        error.Metadata.Should().ContainKeys("Field", "Resource", "AttemptedValue");
        error.Metadata!["Field"].Should().Be("email");
        error.Metadata["Resource"].Should().Be("User");
        error.Metadata["AttemptedValue"].Should().BeNull();
    }

    public static IEnumerable<object[]> ValidCodeData =>
    [
        ["Auth.InvalidToken"],
        ["Validation.FieldRequired"],
        ["General.Unexpected"],
        ["A.B"],
        ["A.B.C.D"]
    ];

    [Theory(DisplayName = "Factory: should accept valid code formats")]
    [MemberData(nameof(ValidCodeData))]
    public void Factory_ShouldAcceptValidCodeFormats(string validCode)
    {
        var error = Error.BadRequest(validCode, "test");

        output.WriteLine("Testing code: {0}", validCode);

        error.Code.Should().Be(validCode);
    }

    public static IEnumerable<object[]> ErrorFactoryTypeData =>
    [
        [() => Error.BadRequest("Test.Code", "test message"), ErrorType.BadRequest, ResultConstant.StatusCodes.BadRequest],
        [() => Error.Unauthorized("Test.Code", "test message"), ErrorType.Unauthorized, ResultConstant.StatusCodes.Unauthorized],
        [() => Error.Forbidden("Test.Code", "test message"), ErrorType.Forbidden, ResultConstant.StatusCodes.Forbidden],
        [() => Error.NotFound("Test.Code", "test message"), ErrorType.NotFound, ResultConstant.StatusCodes.NotFound],
        [() => Error.Conflict("Test.Code", "test message"), ErrorType.Conflict, ResultConstant.StatusCodes.Conflict],
        [() => Error.Validation("Test.Code", "test message"), ErrorType.Validation, ResultConstant.StatusCodes.UnprocessableEntity],
        [() => Error.Unexpected("Test.Code", "test message"), ErrorType.Unexpected, ResultConstant.StatusCodes.InternalServerError]
    ];

    [Theory(DisplayName = "Factory: {0} should set correct ErrorType and match ResultConstant.StatusCodes")]
    [MemberData(nameof(ErrorFactoryTypeData))]
    public void Factory_ShouldSetCorrectType(
        Func<Error> factory,
        int expectedErrorType,
        int expectedStatusCode)
    {
        Error error = factory();

        output.WriteLine(
            "Expected ErrorType={0}, StatusCode={1}, Actual={2}",
            expectedErrorType, expectedStatusCode, error.Type);

        error.Type.Should().Be(expectedErrorType);
        error.Type.Should().Be(expectedStatusCode);
    }

    [Fact(DisplayName = "Factory: BadRequest should propagate message and code")]
    public void Factory_BadRequest_ShouldPropagateMessageAndCode()
    {
        var error = Error.BadRequest("Validation.EmailRequired", "Email is required.");

        error.Code.Should().Be("Validation.EmailRequired");
        error.Message.Should().Be("Email is required.");
    }

    public static IEnumerable<object?[]> EdgeCaseCodeData =>
    [
        [""],
        [null],
        ["   "]
    ];

    [Theory(DisplayName = "Create: should accept edge case code values")]
    [MemberData(nameof(EdgeCaseCodeData))]
    public void Create_ShouldAcceptEdgeCaseCodeValues(string? code)
    {
        var error = Error.Create(code ?? string.Empty, "message");

        output.WriteLine("Testing edge case code: '{0}'", code ?? "<null>");

        error.Code.Should().Be(code ?? string.Empty);
    }

    [Fact(DisplayName = "Factory: with metadata should include metadata")]
    public void Factory_WithMetadata_ShouldIncludeMetadata()
    {
        var error = Error.NotFound(
            "Product.NotFound",
            "Product not found.",
            ("ResourceId", "123"));

        error.Metadata.Should().ContainKey("ResourceId").WhoseValue.Should().Be("123");
    }
}

