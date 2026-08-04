using Module.Profile.Features.Shared.Addresses.Validators;

namespace Module.Profile.Features.Storefront.Addresses.Create;

public static partial class CreateAddress
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