using Module.Profile.Domain.Notifications;

namespace Module.UnitTests.Profile.Domain.Notifications;

[Trait("Category", "Unit")]
[Trait("Module", "Profiles")]
[Trait("Feature", "NotificationPreferences")]
public class NotificationPreferencesExtensionsTests
{
    [Fact(DisplayName = "Create should return default preferences with all enabled")]
    public void Create_ShouldReturnAllEnabled()
    {
        Result<NotificationPreferences> result = NotificationPreferencesExtensions.Create();

        result.IsSuccess.Should().BeTrue();
        result.Value.EnableSms.Should().BeTrue();
        result.Value.EnableEmail.Should().BeTrue();
        result.Value.EnableNewsfeeds.Should().BeTrue();
    }

    [Theory(DisplayName = "Update should modify the specified preference")]
    [InlineData("Sms", false)]
    [InlineData("Email", false)]
    [InlineData("Newsfeeds", false)]
    public void Update_ShouldModifySpecifiedPreference(string field, bool value)
    {
        Result<NotificationPreferences> result = NotificationPreferencesExtensions.Create();

        Result<NotificationPreferences> updated = field switch
        {
            "Sms" => result.Value.Update(enableSms: value),
            "Email" => result.Value.Update(enableEmail: value),
            "Newsfeeds" => result.Value.Update(enableNewsfeeds: value),
            _ => result
        };

        updated.Value.EnableSms.Should().Be(field != "Sms" || value);
        updated.Value.EnableEmail.Should().Be(field != "Email" || value);
        updated.Value.EnableNewsfeeds.Should().Be(field != "Newsfeeds" || value);
    }

    [Fact(DisplayName = "Update should preserve other fields when updating one")]
    public void Update_ShouldPreserveOtherFields()
    {
        Result<NotificationPreferences> result = NotificationPreferencesExtensions.Create();

        Result<NotificationPreferences> updated = result.Value.Update(enableSms: false);

        updated.Value.EnableSms.Should().BeFalse();
        updated.Value.EnableEmail.Should().BeTrue();
        updated.Value.EnableNewsfeeds.Should().BeTrue();
    }

}