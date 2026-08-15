using Module.Catalog.Features.Admin.Variants.Values.Shared.Models;

namespace Module.Catalog.Features.Admin.Variants.Values.Assign;

public static partial class AssignVariantOptionValues
{
    public sealed record Request : VariantOptionValueCollectionParameters;
}