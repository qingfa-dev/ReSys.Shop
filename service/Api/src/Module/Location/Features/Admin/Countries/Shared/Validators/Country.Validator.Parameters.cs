using Module.Location.Features.Admin.Countries.Shared.Models;

namespace Module.Location.Features.Admin.Countries.Shared.Validators;

public static partial class CountryValidator
{
    public sealed class CountryParametersValidator : AbstractValidator<CountryParameters>
    {
        public CountryParametersValidator()
        {
            RuleFor(expression: x => x.Name).ApplyNameRules();
            RuleFor(expression: x => x.IsoCode).ApplyIsoCodeRules();
            RuleFor(expression: x => x.CallingCode).ApplyCallingCodeRules();
        }
    }

    public static IRuleBuilderOptions<T, CountryParameters> ApplyCountryParametersRules<T>(
        this IRuleBuilder<T, CountryParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(validator: new CountryParametersValidator());
    }
}