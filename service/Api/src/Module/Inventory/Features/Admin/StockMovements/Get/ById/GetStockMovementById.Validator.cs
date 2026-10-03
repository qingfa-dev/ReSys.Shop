namespace Module.Inventory.Features.Admin.StockMovements.Get.ById;

public static partial class GetStockMovementById
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