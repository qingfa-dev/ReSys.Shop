using Module.Profile.Features.Store.Profiles.Shared.Models;
using Module.Profile.Features.Store.Profiles.Shared.Validators;

namespace Module.Profile.Features.Store.Profiles.Create;

public static partial class CreateProfile
{
    // Validate: Profile request fields via shared ApplyProfileRules
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator(ISystemDateTime systemDateTime)
        {
            var profileRules = new InlineValidator<ProfileParameter>();
            profileRules.ApplyProfileRules(systemDateTime);

            RuleFor(x => (ProfileParameter)x.Request)
                .SetValidator(profileRules);
        }
    }
}
