using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Mappings;

public static partial class ProductClassificationMapping
{
    public static T MapToListItem<T>(
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

public record ClassificationItemResponse : ProductClassificationParameters
{
    public string Name { get; init; } = string.Empty;
    public string? PrettyName { get; init; }
    public bool IsAssigned { get; init; }
}