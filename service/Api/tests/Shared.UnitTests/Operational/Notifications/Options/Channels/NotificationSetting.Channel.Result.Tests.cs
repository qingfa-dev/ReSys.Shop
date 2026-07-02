using Shared.Operational.Notifications.Options.Channels;

namespace Shared.UnitTests.Operational.Notifications.Options.Channels;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Notifications")]
public class ChannelResultTests
{
    [Fact(DisplayName = "Failure.SectionRequired should include channel name in code and message")]
    public void Failure_SectionRequired_ShouldIncludeChannelName()
    {
        Error error = ChannelResult.Failure.SectionRequired("Email");

        error.Code.Should().Be("Channel.Email.Section.Required");
        error.Message.Should().Contain("Email");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.NoEnabledProvider should include channel name in code and message")]
    public void Failure_NoEnabledProvider_ShouldIncludeChannelName()
    {
        Error error = ChannelResult.Failure.NoEnabledProvider("Sms");

        error.Code.Should().Be("Channel.Sms.NoEnabledProvider");
        error.Message.Should().Contain("Sms");
        error.Type.Should().Be(ErrorType.Validation);
    }
}
