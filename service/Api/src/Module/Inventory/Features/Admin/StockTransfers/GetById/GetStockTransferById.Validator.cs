namespace Module.Inventory.Features.Admin.StockTransfers.GetById;

public static partial class GetStockTransferById
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
