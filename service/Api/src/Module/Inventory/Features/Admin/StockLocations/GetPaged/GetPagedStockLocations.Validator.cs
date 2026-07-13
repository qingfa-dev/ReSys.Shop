namespace Module.Inventory.Features.Admin.StockLocations.GetPaged;

public static partial class GetPagedStockLocations
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters)
                .NotNull();
        }
    }
}