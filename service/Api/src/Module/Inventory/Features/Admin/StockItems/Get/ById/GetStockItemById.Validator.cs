namespace Module.Inventory.Features.Admin.StockItems.Get.ById;

public static partial class GetStockItemById
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
