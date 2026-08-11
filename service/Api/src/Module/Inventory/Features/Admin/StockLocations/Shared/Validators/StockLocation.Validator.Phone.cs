using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Validators;

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyPhoneRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.PhoneMaxLength)
            .WithErrorCode(StockLocationResult.Errors.PhoneTooLong.Code)
            .WithMessage(StockLocationResult.Errors.PhoneTooLong.Message);
    }
}