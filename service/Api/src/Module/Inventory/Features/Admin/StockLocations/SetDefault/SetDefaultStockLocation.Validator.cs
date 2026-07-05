namespace Module.Inventory.Features.Admin.StockLocations.SetDefault;

public static partial class SetDefaultStockLocation
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
