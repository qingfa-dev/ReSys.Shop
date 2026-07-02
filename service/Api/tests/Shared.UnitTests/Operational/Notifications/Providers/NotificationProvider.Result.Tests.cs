using Shared.Operational.Notifications.Providers;

namespace Shared.UnitTests.Operational.Notifications.Providers;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Notifications")]
public sealed class NotificationProviderResultTests
{
    public static IEnumerable<object[]> ErrorData()
    {
        yield return
        [
            NotificationProviderResult.Failure.SendFailed("TestProvider", "error message"),
            "Provider.TestProvider.SendFailed",
            "Provider TestProvider failed to send message: error message",
            ErrorType.Unexpected
        ];
        yield return
        [
            NotificationProviderResult.Failure.ConfigurationMissing("TestProvider", "ApiKey"),
            "Provider.TestProvider.Configuration.ApiKey.Required",
            "Provider TestProvider requires configuration field 'ApiKey'.",
            ErrorType.Unexpected
        ];
        yield return
        [
            NotificationProviderResult.Failure.RecipientMissing("TestProvider"),
            "Provider.TestProvider.Recipient.Required",
            "Provider TestProvider requires a recipient identifier.",
            ErrorType.Validation
        ];
    }

    [Theory(DisplayName = "Error factory should return correct Code, Message, and Type")]
    [MemberData(nameof(ErrorData))]
    public void ErrorFactory_ShouldReturnCorrectProperties(Error error, string expectedCode, string expectedMessage,
        int expectedType)
    {
        error.Code.Should().Be(expectedCode);
        error.Message.Should().Be(expectedMessage);
        error.Type.Should().Be(expectedType);
    }
}