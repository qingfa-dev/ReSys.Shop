namespace Module.Inventory.Features.Admin.StockLocations.GetById;

public static partial class GetStockLocationById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}