namespace Module.Inventory.Features.Admin.StockItems.Delete;

public static partial class DeleteStockItem
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode("StockItem.Delete.IdRequired")
                .WithMessage("Stock item identifier is required.");
        }
    }
}
