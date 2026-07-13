using Module.Catalog.Domain.Products.Options;
using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Mappings;

public static partial class ProductOptionTypeMapping
{
    public static Result<ProductOptionType> MapToDomain<T>(
        this T item,
        Guid productId)
        where T : ProductOptionTypeAssignmentItem
    {
        return ProductOptionTypeMethod.Create(
            productId: productId,
            optionTypeId: item.OptionTypeId,
            position: item.Position);
    }

    public static void MapToDomain<T>(
        this T item,
        ProductOptionType entity)
        where T : ProductOptionTypeAssignmentItem
    {
        entity.Position = item.Position;
    }
}