using Module.Profile.Features.Admin.Addresses.Shared.Validators;

namespace Module.Profile.Features.Admin.Addresses.Update;

public static partial class UpdateUserAddress
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).ApplyAddressParametersRules();
        }
    }
}
