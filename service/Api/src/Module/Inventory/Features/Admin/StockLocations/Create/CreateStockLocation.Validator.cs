using Module.Inventory.Features.Admin.Shared.Validators;

namespace Module.Inventory.Features.Admin.StockLocations.Create;

public static partial class CreateStockLocation
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