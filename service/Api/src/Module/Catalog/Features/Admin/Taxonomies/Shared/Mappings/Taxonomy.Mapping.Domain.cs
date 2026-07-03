using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxonomies.Shared.Mappings;

public static partial class TaxonomyMapping
{
    // Create:
    public static Result<Taxonomy> MapToDomain<T>(this T request) where T : TaxonomyRequest
    {
    return TaxonomyExtensions.Create(
        request.Name,
        request.Presentation,
        request.Position);
    }

    // Update:
    public static Result MapToDomain<T>(this T request, Taxonomy entity) where T : TaxonomyRequest
    {
        return entity.Update(
            name: request.Name,
            presentation: request.Presentation,
            position: request.Position);
    }


}