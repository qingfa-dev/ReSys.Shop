using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.Inventory.Features.Admin.Shared.Validators;

public static partial class StockReservationValidator
{
    // Validate: Stock reservation parameters rules
    public sealed class StockReservationParametersValidator : AbstractValidator<StockReservationParameters>
    {
        public StockReservationParametersValidator()
        {
            RuleFor(x => x.VariantId)
                .NotEmpty()
                .WithErrorCode("StockReservation.Variant.Required")
                .WithMessage("Variant is required.");

            RuleFor(x => x.Quantity)
                .ApplyQuantityRules();

            RuleFor(x => x.Reason)
                .ApplyReasonRules();
        }
    }

    public static IRuleBuilderOptions<T, StockReservationParameters> ApplyStockReservationParametersRules<T>(
        this IRuleBuilder<T, StockReservationParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new StockReservationParametersValidator());
    }
}
