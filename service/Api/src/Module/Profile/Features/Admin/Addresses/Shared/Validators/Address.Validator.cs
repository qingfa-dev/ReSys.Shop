using Module.Profile.Features.Admin.Addresses.Shared.Models;

namespace Module.Profile.Features.Admin.Addresses.Shared.Validators;

public static class AddressValidator
{
    public sealed class AddressParametersValidator : AbstractValidator<AddressParameters>
    {
        public AddressParametersValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Address1).NotEmpty().MaximumLength(500);
            RuleFor(x => x.City).NotEmpty().MaximumLength(255);
        }
    }

    public static IRuleBuilderOptions<T, AddressParameters> ApplyAddressParametersRules<T>(
        this IRuleBuilder<T, AddressParameters> ruleBuilder)
    {
        return ruleBuilder.NotNull().SetValidator(new AddressParametersValidator());
    }
}
