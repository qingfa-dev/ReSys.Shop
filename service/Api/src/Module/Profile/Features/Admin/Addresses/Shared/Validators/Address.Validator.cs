using Module.Profile.Domain.Addresses;
using Module.Profile.Features.Admin.Addresses.Shared.Models;

using Shared.Application.Contracts.Location;

namespace Module.Profile.Features.Admin.Addresses.Shared.Validators;

public static class AddressValidator
{
    public static void ApplyAddressRules<T>(this AbstractValidator<T> validator, ISender sender)
        where T : AddressParameters
    {
        validator.RuleFor(x => x.FirstName).ApplyAddressFirstNameRules();
        validator.RuleFor(x => x.Address1).ApplyAddress1Rules();
        validator.RuleFor(x => x.City).ApplyAddressCityRules();
        validator.RuleFor(x => x.CountryName).ApplyAddressCountryNameRules();

        validator.RuleFor(x => x.CountryCode)
            .MustAsync(async (iso, ct) =>
            {
                var result = await sender.Send(new CountryExistsByIsoQuery(iso!), ct);
                return result.IsSuccess && result.Value;
            })
            .When(x => !string.IsNullOrEmpty(x.CountryCode))
            .WithMessage("Country code does not exist.");

        validator.RuleFor(x => x.StateCode)
            .MustAsync(async (address, stateCode, ct) =>
            {
                if (string.IsNullOrEmpty(address.CountryCode) || string.IsNullOrEmpty(stateCode))
                    return true;
                var result = await sender.Send(new StateExistsByIsoQuery(address.CountryCode, stateCode), ct);
                return result.IsSuccess && result.Value;
            })
            .When(x => !string.IsNullOrEmpty(x.StateCode))
            .WithMessage("State code does not exist for the given country.");
    }

    public static IRuleBuilderOptions<T, AddressParameters> ApplyAddressParametersRules<T>(
        this IRuleBuilder<T, AddressParameters> ruleBuilder, ISender sender)
    {
        var inline = new InlineValidator<AddressParameters>();
        inline.ApplyAddressRules(sender);
        return ruleBuilder.NotNull().SetValidator(inline);
    }
}
