using Module.Customer.Domain;
using Module.Customer.Domain.Notifications;
using Module.Customer.Domain.Preferences;
using Module.Customer.Features.Shared.Profiles.Models;

namespace Module.Customer.Features.Shared.Profiles.Validators;

public static partial class ProfileValidator
{
    public static void ApplyProfileRules<T>(this AbstractValidator<T> validator, ISystemDateTime systemDateTime)
        where T : ProfileParameters
    {
        validator.RuleFor(x => x.FirstName).ApplyFirstNameRules();
        validator.RuleFor(x => x.LastName).ApplyLastNameRules();
        validator.RuleFor(x => x.DateOfBirth).ApplyDateOfBirthRules(systemDateTime);

        validator.RuleFor(x => x.Preferences)
            .SetValidator(new ProfilePreferencesValidator()!);

        validator.RuleFor(x => x.Notifications)
            .SetValidator(new ProfileNotificationPreferencesValidator()!);
    }
}

public class ProfilePreferencesValidator : AbstractValidator<ProfilePreferences>
{
    public ProfilePreferencesValidator()
    {
        RuleFor(x => x.PreferredStyle).ApplyPreferredStyleRules();
        RuleFor(x => x.PreferredFit).ApplyPreferredFitRules();
        RuleFor(x => x.FavoriteColors).ApplyFavoriteColorsRules();
        RuleFor(x => x.FavoriteCategories).ApplyFavoriteCategoriesRules();
        RuleFor(x => x.PreferredBrands).ApplyPreferredBrandsRules();
        RuleFor(x => x.SizeTop).ApplySizeTopRules();
        RuleFor(x => x.SizeBottom).ApplySizeBottomRules();
        RuleFor(x => x.ShoeSize).ApplyShoeSizeRules();
    }
}

public class ProfileNotificationPreferencesValidator : AbstractValidator<ProfileNotificationPreferences>
{
    public ProfileNotificationPreferencesValidator()
    {
        RuleFor(x => x.EnableSms).ApplyEnableSmsRules();
        RuleFor(x => x.EnableEmail).ApplyEnableEmailRules();
        RuleFor(x => x.EnableNewsfeeds).ApplyEnableNewsfeedsRules();
    }
}