using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Validators;

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyPostalCodeRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.PostalCodeMaxLength)
            .WithErrorCode(StockLocationResult.Failure.PostalCodeTooLong.Code)
            .WithMessage(StockLocationResult.Failure.PostalCodeTooLong.Message);
    }
}