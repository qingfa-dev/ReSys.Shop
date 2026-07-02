namespace Shared.UnitTests.Application.Models.Errors;

public sealed class ErrorTypeTests(ITestOutputHelper output)
{
    public static IEnumerable<object[]> ErrorTypeData =>
    [
        [ErrorType.BadRequest, ResultConstant.StatusCodes.BadRequest, nameof(ResultConstant.StatusCodes.BadRequest)],
        [ErrorType.Unauthorized, ResultConstant.StatusCodes.Unauthorized, nameof(ResultConstant.StatusCodes.Unauthorized)],
        [ErrorType.Forbidden, ResultConstant.StatusCodes.Forbidden, nameof(ResultConstant.StatusCodes.Forbidden)],
        [ErrorType.NotFound, ResultConstant.StatusCodes.NotFound, nameof(ResultConstant.StatusCodes.NotFound)],
        [ErrorType.Conflict, ResultConstant.StatusCodes.Conflict, nameof(ResultConstant.StatusCodes.Conflict)],
        [ErrorType.Validation, ResultConstant.StatusCodes.UnprocessableEntity, nameof(ResultConstant.StatusCodes.UnprocessableEntity)],
        [ErrorType.Unexpected, ResultConstant.StatusCodes.InternalServerError, nameof(ResultConstant.StatusCodes.InternalServerError)]
    ];

    [Theory(DisplayName = "ErrorType.{2} should match ResultConstant.StatusCodes.{2}")]
    [MemberData(nameof(ErrorTypeData))]
    public void ErrorType_ShouldMatchResultConstantStatusCode(
        int errorTypeValue,
        int expectedStatusCode,
        string statusCodeName)
    {
        output.WriteLine(
            "ErrorType value: {0}, expected from {1}: {2}",
            errorTypeValue, statusCodeName, expectedStatusCode);

        errorTypeValue.Should().Be(expectedStatusCode);
    }

    [Fact(DisplayName = "All ErrorType constants should be unique")]
    public void AllConstants_ShouldBeUnique()
    {
        var values = new[]
        {
            ErrorType.BadRequest,
            ErrorType.Unauthorized,
            ErrorType.Forbidden,
            ErrorType.NotFound,
            ErrorType.Conflict,
            ErrorType.Validation,
            ErrorType.Unexpected
        };

        output.WriteLine("Total unique types: {0}", values.Distinct().Count());

        values.Distinct().Count().Should().Be(values.Length);
    }
}
