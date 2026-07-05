namespace Module.Inventory.Features.Admin.StockItems.GetPaged;

public static partial class GetPagedStockItems
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters)
                .NotNull();
        }
    }
}
