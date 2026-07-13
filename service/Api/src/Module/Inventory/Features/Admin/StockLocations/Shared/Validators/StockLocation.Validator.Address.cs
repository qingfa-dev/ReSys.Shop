using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Features.Admin.StockLocations.Shared.Validators;

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyAddressRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.AddressMaxLength)
            .WithErrorCode(StockLocationResult.Failure.AddressTooLong.Code)
            .WithMessage(StockLocationResult.Failure.AddressTooLong.Message);
    }
}