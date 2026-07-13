using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Classifications.Revoke;

public static partial class RevokeProductClassifications
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.Classifications.Revoke.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RevokeProductClassifications))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Classifications.Revoke.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Classifications.Revoke.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Classifications.Revoke.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}