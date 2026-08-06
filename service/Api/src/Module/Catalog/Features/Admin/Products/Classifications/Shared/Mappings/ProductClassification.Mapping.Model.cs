using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Products.Classifications.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Mappings;

public static partial class ProductClassificationMapping
{
    public static T MapToClassificationListItem<T>(
        this Taxon taxon,
        bool isAssigned,
        int position = 0)
        where T : ClassificationItemResponse, new()
    {
        return new T
        {
            TaxonId = taxon.Id,
            Name = taxon.Name,
            PrettyName = taxon.PrettyName,
            IsAssigned = isAssigned,
            Position = isAssigned ? position : 0
        };
    }
}

