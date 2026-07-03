using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxonomies.Shared.Mappings;

public static partial class TaxonomyMapping
{
    // List:
    public static T MapToListItem<T>(this Taxonomy entity) where T : TaxonomyListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name,
            Presentation = entity.Presentation,
            Position = entity.Position,
            TaxonsCount = entity.Taxons is not null ? entity.Taxons.Count : 0,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc
        };
    }

    // Detail:
    public static T MapToDetail<T>(this Taxonomy entity) where T : TaxonomyDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name,
            Presentation = entity.Presentation,
            Position = entity.Position,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc
        };
    }
}