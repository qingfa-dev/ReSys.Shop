using Module.Profile.Features.Admin.Addresses.Shared.Validators;

namespace Module.Profile.Features.Store.Addresses.Create;

public static partial class CreateAddress
{
    // ============ COMMAND VALIDATOR ============
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyAddressParametersRules();
        }
    }
}