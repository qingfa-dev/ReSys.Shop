using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Shared.Models;

namespace Module.Profile.Features.Admin.Addresses.Shared.Validators;

public static class AddressValidator
{
    public static void ApplyAddressRules<T>(this AbstractValidator<T> validator)
        where T : AddressParameters
    {
        validator.RuleFor(x => x.FirstName).ApplyAddressFirstNameRules();
        validator.RuleFor(x => x.Address1).ApplyAddress1Rules();
        validator.RuleFor(x => x.City).ApplyAddressCityRules();
        validator.RuleFor(x => x.CountryName).ApplyAddressCountryNameRules();
    }

    public static IRuleBuilderOptions<T, AddressParameters> ApplyAddressParametersRules<T>(
        this IRuleBuilder<T, AddressParameters> ruleBuilder)
    {
        var inline = new InlineValidator<AddressParameters>();
        inline.ApplyAddressRules();
        return ruleBuilder.NotNull().SetValidator(inline);
    }
}
