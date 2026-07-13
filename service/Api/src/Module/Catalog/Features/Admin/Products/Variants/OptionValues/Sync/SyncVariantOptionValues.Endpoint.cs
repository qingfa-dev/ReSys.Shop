using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Sync;

public static partial class SyncVariantOptionValues
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.Products.Variants.OptionValues.Sync.Route, async (
                Guid variantId,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(variantId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(SyncVariantOptionValues))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Variants.OptionValues.Sync.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.OptionValues.Sync.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.OptionValues.Sync.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}