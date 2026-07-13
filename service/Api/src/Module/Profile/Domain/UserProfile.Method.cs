using Module.Profile.Domain.Addresses;
using Module.Profile.Domain.Notifications;
using Module.Profile.Domain.Preferences;

using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Domain;

public static class UserProfileMethod
{
    #region Factory Methods

    public static Result<UserProfile> Create(string firstName,
        string lastName,
        string email,
        string? phoneNumber = null,
        Guid? userId = null,
        DateTimeOffset? dateOfBirth = null,
        string? gender = null,
        string? bio = null,
        string? avatarUrl = null,
        UserPreferences? preferences = null,
        NotificationPreferences? notifications = null,
        bool acceptsEmailMarketing = false,
        string? internalNoteHtml = null,
        Guid? defaultBillingAddressId = null,
        Guid? defaultShippingAddressId = null,
        int ordersCount = 0,
        decimal totalSpent = 0,
        DateTimeOffset? lastOrderCompletedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return UserProfileResult.Failure.FirstNameRequired;
        if (firstName.Length > UserProfileConstant.Constraints.MaxFirstNameLength)
            return UserProfileResult.Failure.FirstNameTooLong;

        if (string.IsNullOrWhiteSpace(lastName))
            return UserProfileResult.Failure.LastNameRequired;
        if (lastName.Length > UserProfileConstant.Constraints.MaxLastNameLength)
            return UserProfileResult.Failure.LastNameTooLong;

        if (string.IsNullOrWhiteSpace(email))
            return UserProfileResult.Failure.EmailRequired;
        if (email.Length > UserProfileConstant.Constraints.MaxEmailLength)
            return UserProfileResult.Failure.EmailTooLong;

        if (phoneNumber?.Length > UserProfileConstant.Constraints.MaxPhoneNumberLength)
            return UserProfileResult.Failure.PhoneNumberTooLong;

        if (gender?.Length > UserProfileConstant.Constraints.MaxGenderLength)
            return UserProfileResult.Failure.GenderTooLong;

        if (bio?.Length > UserProfileConstant.Constraints.MaxBioLength)
            return UserProfileResult.Failure.BioTooLong;

        if (avatarUrl?.Length > UserProfileConstant.Constraints.MaxAvatarUrlLength)
            return UserProfileResult.Failure.AvatarUrlTooLong;

        if (internalNoteHtml?.Length > UserProfileConstant.Constraints.MaxInternalNoteLength)
            return UserProfileResult.Failure.InternalNoteTooLong;

        return new UserProfile
        {
            UserId = userId ?? Guid.Empty,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Bio = bio,
            AvatarUrl = avatarUrl,
            Preferences = preferences ?? UserPreferences.Empty,
            Notifications = notifications ?? NotificationPreferences.Default,
            IsActive = UserProfileConstant.Defaults.IsActive,
            AcceptsEmailMarketing = acceptsEmailMarketing,
            InternalNoteHtml = internalNoteHtml,
            DefaultBillingAddressId = defaultBillingAddressId,
            DefaultShippingAddressId = defaultShippingAddressId,
            OrdersCount = ordersCount,
            TotalSpent = totalSpent,
            LastOrderCompletedAtUtc = lastOrderCompletedAtUtc,
        };
    }

    #endregion

    #region Update

    public static Result<UserProfile> Update(
        this UserProfile profile,
        string? firstName = default,
        string? lastName = default,
        string? email = default,
        string? phoneNumber = default,
        DateTimeOffset? dateOfBirth = default,
        string? gender = default,
        string? bio = default,
        string? avatarUrl = default,
        UserPreferences? preferences = default,
        NotificationPreferences? notifications = default,
        bool? acceptsEmailMarketing = default,
        string? internalNoteHtml = default,
        Guid? defaultBillingAddressId = default,
        Guid? defaultShippingAddressId = default)
    {
        if (firstName is not null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                return UserProfileResult.Failure.FirstNameRequired;
            profile.FirstName = firstName;
        }

        if (lastName is not null)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                return UserProfileResult.Failure.LastNameRequired;
            profile.LastName = lastName;
        }

        if (email is not null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return UserProfileResult.Failure.EmailRequired;
            profile.Email = email;
        }

        if (phoneNumber is not null)
            profile.PhoneNumber = phoneNumber;

        if (dateOfBirth.HasValue)
            profile.DateOfBirth = dateOfBirth.Value;

        if (gender is not null)
        {
            if (gender.Length > UserProfileConstant.Constraints.MaxGenderLength)
                return UserProfileResult.Failure.GenderTooLong;
            profile.Gender = gender;
        }

        if (bio is not null)
        {
            if (bio.Length > UserProfileConstant.Constraints.MaxBioLength)
                return UserProfileResult.Failure.BioTooLong;
            profile.Bio = bio;
        }

        if (avatarUrl is not null)
        {
            if (avatarUrl.Length > UserProfileConstant.Constraints.MaxAvatarUrlLength)
                return UserProfileResult.Failure.AvatarUrlTooLong;
            profile.AvatarUrl = avatarUrl;
        }

        if (preferences is not null)
            profile.Preferences = preferences;

        if (notifications is not null)
            profile.Notifications = notifications;

        if (acceptsEmailMarketing.HasValue)
            profile.AcceptsEmailMarketing = acceptsEmailMarketing.Value;

        if (internalNoteHtml is not null)
        {
            if (internalNoteHtml.Length > UserProfileConstant.Constraints.MaxInternalNoteLength)
                return UserProfileResult.Failure.InternalNoteTooLong;
            profile.InternalNoteHtml = internalNoteHtml;
        }

        if (defaultBillingAddressId.HasValue)
            profile.DefaultBillingAddressId = defaultBillingAddressId.Value;

        if (defaultShippingAddressId.HasValue)
            profile.DefaultShippingAddressId = defaultShippingAddressId.Value;

        return profile;
    }

    #endregion

    #region Address Management

    public static bool HasDefaultBillingAddress(this UserProfile profile)
    {
        return profile.DefaultBillingAddressId.HasValue;
    }

    public static bool HasDefaultShippingAddress(this UserProfile profile)
    {
        return profile.DefaultShippingAddressId.HasValue;
    }

    public static Address? GetDefaultBillingAddress(this UserProfile profile)
    {
        return profile.DefaultBillingAddressId is not null
            ? profile.Addresses.FirstOrDefault(a => a.Id == profile.DefaultBillingAddressId.Value)
            : profile.Addresses.FirstOrDefault(a => a.AddressType == AddressType.Billing && a.IsDefault);
    }

    public static Address? GetDefaultShippingAddress(this UserProfile profile)
    {
        return profile.DefaultShippingAddressId is not null
            ? profile.Addresses.FirstOrDefault(a => a.Id == profile.DefaultShippingAddressId.Value)
            : profile.Addresses.FirstOrDefault(a => a.AddressType == AddressType.Shipping && a.IsDefault);
    }

    public static int AddressCountByType(this UserProfile profile, AddressType type)
    {
        return profile.Addresses.Count(a => a.AddressType == type);
    }

    public static bool CanAddAddressOfType(this UserProfile profile, AddressType type)
    {
        if (profile.Addresses.Count >= UserProfileConstant.Constraints.MaxAddressesCount)
            return false;

        int typeCount = profile.Addresses.Count(a => a.AddressType == type);
        return typeCount < UserProfileConstant.Constraints.MaxAddressesCountPerType;
    }

    public static Result<UserProfile> AddAddress(this UserProfile profile, Address address)
    {
        if (profile.Addresses.Count >= UserProfileConstant.Constraints.MaxAddressesCount)
            return AddressResult.Failure.MaxAddressesReached;

        if (profile.Addresses.Any(a => a.AddressType == address.AddressType && a.Id == address.Id))
            return AddressResult.Failure.DuplicateAddress;

        int typeCount = profile.Addresses.Count(a => a.AddressType == address.AddressType);
        if (typeCount >= UserProfileConstant.Constraints.MaxAddressesCountPerType)
            return AddressResult.Failure.MaxAddressesPerTypeReached;

        profile.Addresses.Add(address);
        return profile;
    }

    public static Result<UserProfile> RemoveAddress(this UserProfile profile, Guid addressId)
    {
        Address? address = profile.Addresses.FirstOrDefault(a => a.Id == addressId);

        if (address is null)
            return AddressResult.Failure.NotFound;

        if (address.IsDefault)
            address.IsDefault = false;

        profile.Addresses.Remove(address);
        return profile;
    }

    public static Result<UserProfile> SetDefaultAddress(this UserProfile profile, Guid addressId)
    {
        Address? address = profile.Addresses.FirstOrDefault(a => a.Id == addressId);

        if (address is null)
            return AddressResult.Failure.NotFound;

        foreach (Address addr in profile.Addresses.Where(a => a.AddressType == address.AddressType))
            addr.IsDefault = false;

        address.IsDefault = true;
        return profile;
    }

    public static Result<Address> GetDefaultAddress(this UserProfile profile, AddressType type)
    {
        Address? address = profile.Addresses.FirstOrDefault(a => a.AddressType == type && a.IsDefault);

        if (address is null)
            return AddressResult.Failure.NotFound;

        return address;
    }

    public static Result<UserProfile> UpdateAddress(this UserProfile profile, Guid addressId,
        Action<Address> updateAction)
    {
        Address? address = profile.Addresses.FirstOrDefault(a => a.Id == addressId);

        if (address is null)
            return AddressResult.Failure.NotFound;

        updateAction(address);
        return profile;
    }

    #endregion

    #region Preferences

    public static Result<UserProfile> WithPreferredStyle(this UserProfile profile, string? style)
    {
        if (style?.Length > UserPreferenceConstant.Constraints.MaxPreferredStyleLength)
            return UserPreferencesResult.Failure.StyleTooLong;

        profile.Preferences.PreferredStyle = style ?? UserPreferenceConstant.Defaults.PreferredStyle;
        return profile;
    }

    public static Result<UserProfile> WithPreferredFit(this UserProfile profile, string? fit)
    {
        if (fit?.Length > UserPreferenceConstant.Constraints.MaxPreferredFitLength)
            return UserPreferencesResult.Failure.FitTooLong;

        profile.Preferences.PreferredFit = fit ?? UserPreferenceConstant.Defaults.PreferredFit;
        return profile;
    }

    public static Result<UserProfile> WithFavoriteColors(this UserProfile profile, List<string> colors)
    {
        if (colors.Count > UserPreferenceConstant.Constraints.MaxFavoriteColorsPerUser)
            return UserPreferencesResult.Failure.TooManyFavoriteColors;

        profile.Preferences.FavoriteColors = colors;
        return profile;
    }

    public static Result<UserProfile> WithFavoriteCategories(this UserProfile profile, List<string> categories)
    {
        if (categories.Count > UserPreferenceConstant.Constraints.MaxFavoriteCategoriesPerUser)
            return UserPreferencesResult.Failure.TooManyFavoriteCategories;

        profile.Preferences.FavoriteCategories = categories;
        return profile;
    }

    public static Result<UserProfile> WithPreferredBrands(this UserProfile profile, List<string> brands)
    {
        if (brands.Count > UserPreferenceConstant.Constraints.MaxPreferredBrandsPerUser)
            return UserPreferencesResult.Failure.TooManyPreferredBrands;

        profile.Preferences.PreferredBrands = brands;
        return profile;
    }

    public static Result<UserProfile> WithSizeTop(this UserProfile profile, string? sizeTop)
    {
        if (sizeTop?.Length > UserPreferenceConstant.Constraints.MaxSizeTopLength)
            return UserPreferencesResult.Failure.InvalidSizeTop;

        profile.Preferences.SizeTop = sizeTop;
        return profile;
    }

    public static Result<UserProfile> WithSizeBottom(this UserProfile profile, string? sizeBottom)
    {
        if (sizeBottom?.Length > UserPreferenceConstant.Constraints.MaxSizeBottomLength)
            return UserPreferencesResult.Failure.InvalidSizeBottom;

        profile.Preferences.SizeBottom = sizeBottom;
        return profile;
    }

    public static Result<UserProfile> WithShoeSize(this UserProfile profile, string? shoeSize)
    {
        if (shoeSize?.Length > UserPreferenceConstant.Constraints.MaxShoeSizeLength)
            return UserPreferencesResult.Failure.InvalidShoeSize;

        profile.Preferences.ShoeSize = shoeSize;
        return profile;
    }

    #endregion

    #region Notifications

    public static Result<UserProfile> WithSmsNotification(this UserProfile profile, bool enableSms)
    {
        profile.Notifications.EnableSms = enableSms;
        return profile;
    }

    public static Result<UserProfile> WithEmailNotification(this UserProfile profile, bool enableEmail)
    {
        profile.Notifications.EnableEmail = enableEmail;
        return profile;
    }

    public static Result<UserProfile> WithNewsfeedNotification(this UserProfile profile, bool enableNewsfeeds)
    {
        profile.Notifications.EnableNewsfeeds = enableNewsfeeds;
        return profile;
    }

    #endregion

    #region Commerce

    public static Result<UserProfile> WithAcceptsEmailMarketing(this UserProfile profile, bool accepts)
    {
        profile.AcceptsEmailMarketing = accepts;
        return profile;
    }

    public static Result<UserProfile> WithInternalNoteHtml(this UserProfile profile, string? note)
    {
        if (note?.Length > UserProfileConstant.Constraints.MaxInternalNoteLength)
            return UserProfileResult.Failure.InternalNoteTooLong;
        profile.InternalNoteHtml = note;
        return profile;
    }

    public static Result<UserProfile> WithDefaultBillingAddressId(this UserProfile profile, Guid? addressId)
    {
        profile.DefaultBillingAddressId = addressId;
        return profile;
    }

    public static Result<UserProfile> WithDefaultShippingAddressId(this UserProfile profile, Guid? addressId)
    {
        profile.DefaultShippingAddressId = addressId;
        return profile;
    }

    public static Result<UserProfile> WithOrdersCount(this UserProfile profile, int count)
    {
        profile.OrdersCount = count;
        return profile;
    }

    public static Result<UserProfile> WithTotalSpent(this UserProfile profile, decimal total)
    {
        profile.TotalSpent = total;
        return profile;
    }

    public static Result<UserProfile> WithLastOrderCompletedAtUtc(this UserProfile profile,
        DateTimeOffset? completedAt)
    {
        profile.LastOrderCompletedAtUtc = completedAt;
        return profile;
    }

    public static Result<UserProfile> UpdateCommerceProfile(
        this UserProfile profile,
        bool acceptsEmailMarketing,
        string? internalNoteHtml = null,
        Guid? defaultBillingAddressId = null,
        Guid? defaultShippingAddressId = null)
    {
        profile.AcceptsEmailMarketing = acceptsEmailMarketing;

        if (internalNoteHtml is not null)
        {
            if (internalNoteHtml.Length > UserProfileConstant.Constraints.MaxInternalNoteLength)
                return UserProfileResult.Failure.InternalNoteTooLong;
            profile.InternalNoteHtml = internalNoteHtml;
        }

        profile.DefaultBillingAddressId = defaultBillingAddressId ?? profile.DefaultBillingAddressId;
        profile.DefaultShippingAddressId = defaultShippingAddressId ?? profile.DefaultShippingAddressId;

        return profile;
    }

    public static bool IsActiveCustomer(this UserProfile profile)
    {
        return profile.IsActive && profile.OrdersCount > 0;
    }

    public static void RecordCompletedOrder(this UserProfile profile, decimal orderTotal)
    {
        profile.OrdersCount++;
        profile.TotalSpent += orderTotal;
        profile.LastOrderCompletedAtUtc = DateTimeOffset.UtcNow;
    }

    public static decimal LifetimeValue(this UserProfile profile)
    {
        return profile.TotalSpent;
    }

    public static decimal AverageOrderValue(this UserProfile profile)
    {
        return profile.OrdersCount > 0
            ? profile.TotalSpent / profile.OrdersCount
            : 0;
    }

    public static int? DaysSinceLastOrder(this UserProfile profile)
    {
        if (profile.LastOrderCompletedAtUtc is null)
            return null;

        return (int)(DateTimeOffset.UtcNow - profile.LastOrderCompletedAtUtc.Value).TotalDays;
    }

    public static bool IsRepeatCustomer(this UserProfile profile)
    {
        return profile.OrdersCount > 1;
    }

    public static bool HasEverOrdered(this UserProfile profile)
    {
        return profile.OrdersCount > 0 && profile.LastOrderCompletedAtUtc is not null;
    }

    public static bool WasRecentlyActive(this UserProfile profile, int withinDays = 30)
    {
        if (profile.User.LastSignInAtUtc is null)
            return false;

        return (DateTimeOffset.UtcNow - profile.User.LastSignInAtUtc.Value).TotalDays <= withinDays;
    }

    #endregion

    #region Sign In

    public static Result<UserProfile> RecordSignIn(this UserProfile profile, string currentIp)
    {
        UserMethod.RecordSignIn(profile.User, currentIp);

        return profile;
    }

    public static void RecordFailedAttempt(this UserProfile profile)
    {
        UserMethod.RecordFailedAttempt(profile.User);
    }

    public static void ResetFailedAttempts(this UserProfile profile)
    {
        UserMethod.ResetFailedAttempts(profile.User);
    }

    #endregion

    #region Status

    public static void Deactivate(this UserProfile profile)
    {
        profile.IsActive = false;
    }

    public static void Activate(this UserProfile profile)
    {
        profile.IsActive = true;
    }

    public static bool IsUsable(this UserProfile profile)
    {
        return profile.IsActive && !string.IsNullOrWhiteSpace(profile.Email);
    }

    #endregion

    #region Display

    public static string DisplayName(this UserProfile profile)
    {
        return !string.IsNullOrWhiteSpace(profile.FirstName)
            ? $"{profile.FirstName} {profile.LastName}".Trim()
            : profile.Email;
    }

    public static string FullName(this UserProfile profile)
    {
        return $"{profile.FirstName} {profile.LastName}".Trim();
    }

    #endregion

    #region Roles

    public static bool HasRole(this UserProfile profile, string roleName, IReadOnlyCollection<Role> roles)
    {
        return roles.Any(r =>
            !string.IsNullOrEmpty(r.Name) && r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsAdmin(this UserProfile profile, IReadOnlyCollection<Role> roles)
    {
        return roles.Any(r => !string.IsNullOrEmpty(r.Name) &&
                              r.Name.Equals(RoleConstant.Defaults.Admin, StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasAnyRole(this UserProfile profile, IReadOnlyCollection<string> roleNames,
        IReadOnlyCollection<Role> userRoles)
    {
        return userRoles.Any(r => !string.IsNullOrEmpty(r.Name) &&
                                  roleNames.Any(n =>
                                      !string.IsNullOrEmpty(n) &&
                                      r.Name.Equals(n, StringComparison.OrdinalIgnoreCase)));
    }

    #endregion
}