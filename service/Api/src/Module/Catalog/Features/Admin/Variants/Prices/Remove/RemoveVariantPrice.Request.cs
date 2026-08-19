using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Prices.Remove;

public static partial class RemoveVariantPrice
{
    public sealed record Request : VariantPriceActionParameters;
}
