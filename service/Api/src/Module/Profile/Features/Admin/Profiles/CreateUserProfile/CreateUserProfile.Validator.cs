using Module.Profile.Features.Admin.Profiles.Shared.Validators;

namespace Module.Profile.Features.Admin.Profiles.CreateUserProfile;

public static partial class CreateUserProfile
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).ApplyProfileRequestRules();
        }
    }
}
