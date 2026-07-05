namespace Module.Inventory.Features.Admin.StockLocations.Get.ById;

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
