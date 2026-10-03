namespace Module.Inventory.Features.Admin.StockItems.GetById;

public static partial class GetStockItemById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
