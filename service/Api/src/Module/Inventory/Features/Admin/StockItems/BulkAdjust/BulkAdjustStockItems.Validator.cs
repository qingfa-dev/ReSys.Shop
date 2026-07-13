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
                RuleFor(x => x.Request.Items)
                    .NotEmpty()
                    .WithErrorCode("BulkAdjust.Items.Required")
                    .WithMessage("At least one adjustment item is required.");

                RuleForEach(x => x.Request.Items).ChildRules(item =>
                {
                    item.RuleFor(i => i.StockItemId)
                        .NotEmpty()
                        .WithErrorCode("BulkAdjust.StockItem.Required")
                        .WithMessage("Stock item identifier is required.");

                    item.RuleFor(i => i.Quantity)
                        .NotEqual(0)
                        .WithErrorCode("BulkAdjust.Quantity.Required")
                        .WithMessage("Adjustment quantity must not be zero.");
                });
            });
        }
    }
}