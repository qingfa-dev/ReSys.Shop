using Module.Catalog.Domain.Products.Classifications;
using Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Classifications.Shared.Mappings;

public static partial class ProductClassificationMapping
{
    public static Result<Classification> MapToDomain<T>(
        this T item,
        Guid productId)
        where T : ProductClassificationAssignmentItem
    {
        return ClassificationMethod.Create(
            productId: productId,
            taxonId: item.TaxonId,
            position: item.Position);
    }

    public static void MapToDomain<T>(
        this T item,
        Classification entity)
        where T : ProductClassificationAssignmentItem
    {
        entity.Position = item.Position;
    }
}