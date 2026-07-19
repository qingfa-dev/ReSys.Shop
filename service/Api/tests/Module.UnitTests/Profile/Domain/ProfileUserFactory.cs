using Module.Profile.Domain;
using Module.Profile.Domain.Notifications;
using Module.Profile.Domain.Preferences;

namespace Module.UnitTests.Profile.Domain;

public static class ProfileUserFactory
{
    public static UserProfile Create(Guid userId)
    {
        return UserProfileMethod.Create(
            firstName: "Test",
            lastName: "User",
            email: "test@example.com",
            userId: userId,
            phoneNumber: "+1234567890",
            dateOfBirth: new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
            preferences: UserPreferenceMethod.Create().Value,
            notifications: NotificationPreferencesMethod.Create().Value
        ).Value;
    }
}