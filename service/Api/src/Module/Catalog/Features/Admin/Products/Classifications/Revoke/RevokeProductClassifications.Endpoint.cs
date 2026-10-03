using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Revoke;

public static partial class RevokeProductClassifications
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.ProductClassifications.Revoke.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.ProductId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RevokeProductClassifications))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.ProductClassifications.Revoke.Permission)
            .WithSummary(CatalogFeature.Admin.ProductClassifications.Revoke.Summary)
            .WithDescription(CatalogFeature.Admin.ProductClassifications.Revoke.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}