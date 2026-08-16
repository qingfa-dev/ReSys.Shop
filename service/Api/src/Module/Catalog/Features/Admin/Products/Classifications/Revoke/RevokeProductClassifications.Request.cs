using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Revoke;

public static partial class RevokeProductClassifications
{
    public sealed record Request : ProductClassificationCollectionParameters;
}