using Module.Customer.Features.Shared.Addresses.Validators;

namespace Module.Customer.Features.Storefront.Addresses.Update;

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