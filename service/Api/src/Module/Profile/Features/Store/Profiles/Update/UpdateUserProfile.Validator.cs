using Module.Profile.Domain;

namespace Module.Profile.Features.Store.Profiles.Update;

public static partial class UpdateUserProfile
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator(ISystemDateTime systemDateTime)
        {
            RuleFor(x => x.FirstName).ApplyFirstNameRules(isRequired: false);
            RuleFor(x => x.LastName).ApplyLastNameRules(isRequired: false);
            RuleFor(x => x.DateOfBirth).ApplyDateOfBirthRules(systemDateTime);
        }
    }
}
