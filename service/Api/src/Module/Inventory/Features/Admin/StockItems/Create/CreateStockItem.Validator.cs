using Module.Inventory.Features.Admin.StockItems.Shared.Validators;

namespace Module.Inventory.Features.Admin.StockItems.Create;

public static partial class CreateStockItem
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request)
                .ApplyStockItemParametersRules();
        }
    }
}