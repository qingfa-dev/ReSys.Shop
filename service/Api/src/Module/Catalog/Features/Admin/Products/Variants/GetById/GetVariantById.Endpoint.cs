using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.GetById;

public static partial class GetVariantById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Products.Variants.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetVariantById))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.Products.Variants.GetById.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.GetById.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}