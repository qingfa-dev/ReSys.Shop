using Module.Location.Features.Shared.Countries.Models;

namespace Module.Location.Features.Shared.Countries.Validators;

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