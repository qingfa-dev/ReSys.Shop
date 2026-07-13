using Module.Inventory.Features.Admin.StockLocations.Shared.Models;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Validators;

public static partial class StockLocationValidator
{
    public sealed class StockLocationParametersValidator : AbstractValidator<StockLocationParameters>
    {
        public StockLocationParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Code).ApplyCodeRules();
            RuleFor(x => x.Address1).ApplyAddressRules();
            RuleFor(x => x.Address2).ApplyAddressRules();
            RuleFor(x => x.City).ApplyCityRules();
            RuleFor(x => x.Phone).ApplyPhoneRules();
            RuleFor(x => x.PostalCode).ApplyPostalCodeRules();
        }
    }

    public static IRuleBuilderOptions<T, StockLocationParameters> ApplyStockLocationParametersRules<T>(
        this IRuleBuilder<T, StockLocationParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new StockLocationParametersValidator());
    }
}