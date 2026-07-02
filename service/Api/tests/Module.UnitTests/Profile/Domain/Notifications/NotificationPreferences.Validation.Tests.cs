using Module.Profile.Domain.Notifications;

namespace Module.UnitTests.Profile.Domain.Notifications;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "NotificationPreferencesValidation")]
public class NotificationPreferencesValidationTests
{
    private sealed class NotificationPreferencesTestModel
    {
        public NotificationPreferences? Notifications { get; set; }
    }

    private sealed class NotificationPreferencesValidatorWrapper : AbstractValidator<NotificationPreferencesTestModel>
    {
        public NotificationPreferencesValidatorWrapper()
        {
            RuleFor(x => x.Notifications).ValidateNotificationPreferences();
        }
    }

    [Fact]
    public void ValidateNotificationPreferences_WhenValid_ShouldPass()
    {
        var validator = new NotificationPreferencesValidatorWrapper();
        var notifications = NotificationPreferences.Create(true, true, true).Value;
        var result = validator.TestValidate(new NotificationPreferencesTestModel { Notifications = notifications });
        result.ShouldNotHaveValidationErrorFor(x => x.Notifications);
    }
}
