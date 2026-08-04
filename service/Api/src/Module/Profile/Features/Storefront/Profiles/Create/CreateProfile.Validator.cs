using Module.Profile.Features.Shared.Profiles.Models;
using Module.Profile.Features.Shared.Profiles.Validators;

namespace Module.Profile.Features.Storefront.Profiles.Create;

public static partial class CreateProfile
{
    // Validate: Profile request fields via shared ApplyProfileRules
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator(ISystemDateTime systemDateTime)
        {
            var profileRules = new InlineValidator<ProfileParameters>();
            profileRules.ApplyProfileRules(systemDateTime);

            RuleFor(x => x.Request)
                .SetValidator(profileRules);
        }
    }
}