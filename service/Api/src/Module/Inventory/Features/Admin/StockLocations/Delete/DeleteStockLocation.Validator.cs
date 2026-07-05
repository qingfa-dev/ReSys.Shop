namespace Module.Inventory.Features.Admin.StockLocations.Delete;

public static partial class DeleteStockLocation
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}
