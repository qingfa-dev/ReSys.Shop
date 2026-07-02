using Shared.Operational.Notifications.Channels.Emails.Options;

namespace Shared.UnitTests.Operational.Notifications.Channels.Email.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Notifications")]
public sealed class EmailChannelSettingResultTests
{
    public static IEnumerable<object[]> ErrorData()
    {
        yield return [EmailChannelSettingResult.Failure.FromEmailRequired, "Notification.Email.FromEmailRequired", "Sender email is required.", ErrorType.Validation
        ];
        yield return [EmailChannelSettingResult.Failure.FromNameRequired, "Notification.Email.FromNameRequired", "Sender name is required.", ErrorType.Validation
        ];
    }

    [Theory(DisplayName = "Error '{0}' should return expected Code, Message, and Type")]
    [MemberData(nameof(ErrorData))]
    public void Error_ShouldReturnExpectedValues(Error error, string expectedCode, string expectedMessage, int expectedType)
    {
        error.Code.Should().Be(expectedCode);
        error.Message.Should().Be(expectedMessage);
        error.Type.Should().Be(expectedType);
    }
}
