using Module.Catalog.Domain.Variants.Prices;

namespace Module.Catalog.Features.Admin.Variants.Prices.Sync;

public static partial class SyncVariantPrices
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Prices)
                .NotEmpty();

            RuleForEach(x => x.Request.Prices)
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.Currency)
                        .ApplyCurrencyRules();
                });
        }
    }
}