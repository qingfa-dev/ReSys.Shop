using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Models;

namespace Module.Inventory.Features.Admin.StockItems.Shared.Validators;

public static partial class StockItemValidator
{
    // Validate: StockItem parameters rules
    public sealed class StockItemParametersValidator : AbstractValidator<StockItemParameters>
    {
        public StockItemParametersValidator()
        {
            RuleFor(x => x.StockLocationId)
                .NotEmpty()
                .WithErrorCode("StockItem.StockLocation.Required")
                .WithMessage("Stock location is required.");

            RuleFor(x => x.VariantId)
                .NotEmpty()
                .WithErrorCode("StockItem.Variant.Required")
                .WithMessage("Variant is required.");

            RuleFor(x => x.CountOnHand)
                .GreaterThanOrEqualTo(0)
                .WithErrorCode(StockItemResult.Errors.NegativeCountOnHand.Code)
                .WithMessage(StockItemResult.Errors.NegativeCountOnHand.Message);
        }
    }

    public static IRuleBuilderOptions<T, StockItemParameters> ApplyStockItemParametersRules<T>(
        this IRuleBuilder<T, StockItemParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new StockItemParametersValidator());
    }
}
