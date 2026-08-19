using Module.Catalog.Features.Admin.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Values.Revoke;

public static partial class RevokeVariantOptionValues
{
    public sealed record Request : VariantOptionValueCollectionParameters;
}