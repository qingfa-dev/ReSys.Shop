namespace Module.Inventory.Features.Admin.StockItems.LowStock;

public static partial class GetLowStockItems
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            When(x => x.Threshold.HasValue, () =>
            {
                RuleFor(x => x.Threshold!.Value)
                    .GreaterThanOrEqualTo(0)
                    .WithErrorCode("LowStock.InvalidThreshold")
                    .WithMessage("Low stock threshold must be non-negative.");
            });
        }
    }
}
