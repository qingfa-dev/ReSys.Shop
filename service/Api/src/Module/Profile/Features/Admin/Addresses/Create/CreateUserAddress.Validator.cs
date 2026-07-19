using Module.Profile.Features.Admin.Addresses.Shared.Validators;

namespace Module.Profile.Features.Admin.Addresses.Create;

public static partial class CreateUserAddress
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).ApplyAddressParametersRules();
        }
    }
}
