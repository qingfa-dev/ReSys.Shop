using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Classifications.Sync;

public static partial class SyncProductClassifications
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.Products.Classifications.Sync.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(SyncProductClassifications))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Classifications.Sync.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Classifications.Sync.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Classifications.Sync.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
