using Shared.Operational.Notifications.Hubs;
using Shared.Operational.Notifications.Templates;

namespace Shared.UnitTests.Operational.Notifications.Hubs;
[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Notifications")]
public sealed class NotificationHubResultTests
{
    public static IEnumerable<object[]> ErrorData()
    {
        yield return
        [
            NotificationHubResult.Failure.NoProvidersConfigured(NotificationChannel.SMS),
            "Notification.SMS.NoProvidersConfigured",
            "No active SMS providers are configured.",
            ErrorType.Unexpected
        ];
        yield return
        [
            NotificationHubResult.Failure.NoProvidersConfigured(NotificationChannel.Email),
            "Notification.Email.NoProvidersConfigured",
            "No active Email providers are configured.",
            ErrorType.Unexpected
        ];
        yield return
        [
            NotificationHubResult.Failure.AllProvidersFailed(NotificationChannel.Email),
            "Notification.Email.AllProvidersFailed",
            "All Email providers failed.",
            ErrorType.Unexpected
        ];
        yield return
        [
            NotificationHubResult.Failure.AllProvidersFailed(NotificationChannel.SMS),
            "Notification.SMS.AllProvidersFailed",
            "All SMS providers failed.",
            ErrorType.Unexpected
        ];
    }
    [Theory(DisplayName = "Error factory should return correct Code, Message, and Type")]
    [MemberData(nameof(ErrorData))]
    public void ErrorFactory_ShouldReturnCorrectProperties(Error error, string expectedCode, string expectedMessage, int expectedType)
    {
        error.Code.Should().Be(expectedCode);
        error.Message.Should().Be(expectedMessage);
        error.Type.Should().Be(expectedType);
    }
}
