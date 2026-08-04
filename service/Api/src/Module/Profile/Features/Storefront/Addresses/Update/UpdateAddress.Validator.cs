using Module.Profile.Features.Shared.Addresses.Validators;

namespace Module.Profile.Features.Storefront.Addresses.Update;

public static partial class UpdateAddress
{
    // ============ COMMAND VALIDATOR ============
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator(ISender sender)
        {
            RuleFor(x => x.Request)
                .ApplyAddressParametersRules(sender);
        }
    }
}