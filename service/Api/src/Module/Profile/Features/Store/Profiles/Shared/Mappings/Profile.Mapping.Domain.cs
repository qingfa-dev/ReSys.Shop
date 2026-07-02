using Module.Profile.Domain;
using Module.Profile.Domain.Notifications;
using Module.Profile.Domain.Preferences;
using Module.Profile.Features.Store.Profile.Shared.Models;

using Shared.Application.Domain.Concerns.Auditable;

namespace Module.Profile.Features.Store.Profile.Shared.Mappings;

public static partial class ProfileMapping
{
    public static UserProfile MapToDomain<T>(this T request) where T : ProfileRequest
    {
        return UserProfileMethod.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            phoneNumber: request.PhoneNumber,
            dateOfBirth: request.DateOfBirth,
            preferences: UserPreferenceMethod.Create(
                request.Preferences?.PreferredStyle,
                request.Preferences?.PreferredFit,
                request.Preferences?.FavoriteColors ?? [],
                request.Preferences?.FavoriteCategories ?? [],
                request.Preferences?.PreferredBrands ?? [],
                request.Preferences?.SizeTop,
                request.Preferences?.SizeBottom,
                request.Preferences?.ShoeSize).Value,
            notifications: NotificationPreferencesExtensions.Create(
                request.Notifications?.EnableSms ?? true,
                request.Notifications?.EnableEmail ?? true,
                request.Notifications?.EnableNewsfeeds ?? true).Value
        ).Value;
    }

    public static void MapToDomain<T>(
        this T request, UserProfile profile) where T : ProfileRequest
    {
        profile.FirstName = request.FirstName;
        profile.LastName = request.LastName;
        profile.Email = request.Email;
        profile.PhoneNumber = request.PhoneNumber;
        profile.DateOfBirth = request.DateOfBirth;

        profile.Preferences = UserPreferenceMethod.Create(
            request.Preferences?.PreferredStyle,
            request.Preferences?.PreferredFit,
            request.Preferences?.FavoriteColors ?? [],
            request.Preferences?.FavoriteCategories ?? [],
            request.Preferences?.PreferredBrands ?? [],
            request.Preferences?.SizeTop,
            request.Preferences?.SizeBottom,
            request.Preferences?.ShoeSize).Value;

        profile.Notifications = NotificationPreferencesExtensions.Create(
            request.Notifications?.EnableSms ?? true,
            request.Notifications?.EnableEmail ?? true,
            request.Notifications?.EnableNewsfeeds ?? true).Value;

        AuditableBehavior.Touch(profile, DateTimeOffset.UtcNow);
    }
}