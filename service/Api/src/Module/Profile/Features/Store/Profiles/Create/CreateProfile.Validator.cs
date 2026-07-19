using Module.Profile.Features.Admin.Profiles.Shared.Models;
using Module.Profile.Features.Admin.Profiles.Shared.Validators;

namespace Module.Profile.Features.Store.Profiles.Create;

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