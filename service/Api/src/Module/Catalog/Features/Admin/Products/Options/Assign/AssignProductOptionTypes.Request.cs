using Module.Catalog.Features.Admin.Products.Options.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Options.Assign;

public static partial class AssignProductOptionTypes
{
    public sealed record Request : ProductOptionTypeCollectionParameters;
}