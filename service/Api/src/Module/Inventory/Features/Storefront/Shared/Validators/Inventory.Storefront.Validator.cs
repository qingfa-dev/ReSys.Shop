using FluentValidation;

using Module.Inventory.Features.Storefront.Shared.Models;

namespace Module.Inventory.Features.Storefront.Shared.Validators;

public static class InventoryStorefrontValidator
{
    public static IRuleBuilderOptions<T, Guid> MustBeValidCartId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder.NotEmpty();

    public static IRuleBuilderOptions<T, IEnumerable<ReserveLineItem>> MustHaveValidLineItems<T>(
        this IRuleBuilder<T, IEnumerable<ReserveLineItem>> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .ForEach(item =>
            {
                item.SetValidator(new InlineValidator<ReserveLineItem>
                {
                    v => v.RuleFor(i => i.VariantId).NotEmpty(),
                    v => v.RuleFor(i => i.Quantity).GreaterThan(0),
                });
            });
    }

    public static IRuleBuilderOptions<T, int> MustBeValidTtlMinutes<T>(
        this IRuleBuilder<T, int> ruleBuilder) =>
        ruleBuilder.GreaterThan(0);
}
