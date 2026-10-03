using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Storefront.Shared.Models;

namespace Module.Catalog.Features.Storefront.Shared.Mappings;

public static class VisualSearchModelMapping
{
    public static T MapToVisualSearchModel<T>(this ModelMetadata source) where T : VisualSearchModelResponse, new()
    {
        return new T
        {
            Id = source.Id,
            Name = source.Name,
            Dimension = source.Dimension,
            Description = source.Description,
            IsOnnx = source.IsOnnx,
        };
    }
}
