using Module.Profile.Domain;
using Module.Profile.Features.Store.Profile.Shared.Models;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Profile.Shared.Mappings;

public static partial class ProfileMapping
{
    public static T MapToDetail<T>(this UserProfile profile, User? user = null) where T : ProfileDetailResponse, new()
    {
        return new T
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = profile.Email,
            PhoneNumber = profile.PhoneNumber ?? string.Empty,
            DateOfBirth = profile.DateOfBirth,
            Preferences =
                new ProfilePreferences
                {
                    PreferredStyle = profile.Preferences.PreferredStyle,
                    PreferredFit = profile.Preferences.PreferredFit,
                    FavoriteColors = profile.Preferences.FavoriteColors,
                    FavoriteCategories = profile.Preferences.FavoriteCategories,
                    PreferredBrands = profile.Preferences.PreferredBrands,
                    SizeTop = profile.Preferences.SizeTop,
                    SizeBottom = profile.Preferences.SizeBottom,
                    ShoeSize = profile.Preferences.ShoeSize
                },
            Notifications =
                new ProfileNotificationPreferences
                {
                    EnableSms = profile.Notifications.EnableSms,
                    EnableEmail = profile.Notifications.EnableEmail,
                    EnableNewsfeeds = profile.Notifications.EnableNewsfeeds
                },
            EmailConfirmed = user?.EmailConfirmed ?? false,
            PhoneNumberConfirmed = user?.PhoneNumberConfirmed ?? false,
            FullName = $"{profile.FirstName} {profile.LastName}"
        };
    }

    public static T MapToListItem<T>(this UserProfile profile, User? user = null)
        where T : ProfileListItemResponse, new()
    {
        return new T
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = profile.Email,
            PhoneNumber = profile.PhoneNumber ?? string.Empty,
            DateOfBirth = profile.DateOfBirth,
            Preferences = new ProfilePreferences
            {
                PreferredStyle = profile.Preferences.PreferredStyle,
                PreferredFit = profile.Preferences.PreferredFit,
                FavoriteColors = profile.Preferences.FavoriteColors,
                FavoriteCategories = profile.Preferences.FavoriteCategories,
                PreferredBrands = profile.Preferences.PreferredBrands,
                SizeTop = profile.Preferences.SizeTop,
                SizeBottom = profile.Preferences.SizeBottom,
                ShoeSize = profile.Preferences.ShoeSize
            },
            Notifications = new ProfileNotificationPreferences
            {
                EnableSms = profile.Notifications.EnableSms,
                EnableEmail = profile.Notifications.EnableEmail,
                EnableNewsfeeds = profile.Notifications.EnableNewsfeeds
            },
            FullName = $"{profile.FirstName} {profile.LastName}"
        };
    }
}
