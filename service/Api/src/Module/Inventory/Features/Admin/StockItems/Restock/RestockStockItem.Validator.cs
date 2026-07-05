namespace Module.Inventory.Features.Admin.StockItems.Restock;

public static partial class RestockStockItem
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Request)
                .NotNull();

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.Quantity)
                    .GreaterThan(0)
                    .WithErrorCode("Restock.InvalidQuantity")
                    .WithMessage("Restock quantity must be greater than zero.");
            });
        }
    }
}
