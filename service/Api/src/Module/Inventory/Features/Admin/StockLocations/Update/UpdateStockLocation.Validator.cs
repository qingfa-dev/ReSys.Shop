using Module.Inventory.Features.Admin.StockLocations.Shared.Validators;

namespace Module.Inventory.Features.Admin.StockLocations.Update;

public static partial class UpdateStockLocation
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyStockLocationParametersRules();
        }
    }
}
