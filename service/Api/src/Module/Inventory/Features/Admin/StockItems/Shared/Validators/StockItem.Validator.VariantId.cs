namespace Module.Inventory.Features.Admin.StockItems.Shared.Validators;

public static partial class StockItemValidator
{
    public static IRuleBuilderOptions<T, Guid> ApplyVariantIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode("StockItem.Variant.Required")
            .WithMessage("Variant is required.");
    }
}
