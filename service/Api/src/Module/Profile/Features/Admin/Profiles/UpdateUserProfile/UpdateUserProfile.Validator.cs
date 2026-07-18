using Module.Profile.Features.Admin.Profiles.Shared.Validators;

namespace Module.Profile.Features.Admin.Profiles.UpdateUserProfile;

public static partial class UpdateUserProfile
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).ApplyProfileRequestRules();
        }
    }
}
