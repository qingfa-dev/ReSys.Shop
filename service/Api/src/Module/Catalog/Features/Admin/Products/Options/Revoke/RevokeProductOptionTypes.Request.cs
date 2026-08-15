using Module.Catalog.Features.Admin.Products.Options.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Options.Revoke;

public static partial class RevokeProductOptionTypes
{
    public sealed record Request : ProductOptionTypeCollectionParameters;
}