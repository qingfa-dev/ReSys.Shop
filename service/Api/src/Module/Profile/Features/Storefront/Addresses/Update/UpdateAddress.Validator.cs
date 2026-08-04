using Module.Profile.Features.Admin.Addresses.Shared.Validators;

namespace Module.Profile.Features.Storefront.Addresses.Update;

public static partial class UpdateAddress
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