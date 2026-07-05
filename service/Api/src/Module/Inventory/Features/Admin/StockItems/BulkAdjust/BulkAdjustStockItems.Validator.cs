namespace Module.Inventory.Features.Admin.StockItems.BulkAdjust;

public static partial class BulkAdjustStockItems
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithErrorCode("BulkAdjust.Request.Required")
                .WithMessage("Adjustment request is required.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.StockItemId)
                    .NotEmpty()
                    .WithErrorCode("BulkAdjust.StockItem.Required")
                    .WithMessage("Stock item identifier is required.");

                RuleFor(x => x.Request.Quantity)
                    .NotEmpty()
                    .WithErrorCode("BulkAdjust.Quantity.Required")
                    .WithMessage("Adjustment quantity is required.");
            });
        }
    }
}
