using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Revoke;

public static partial class RevokeVariantOptionValues
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.Variants.OptionValues.Revoke.Route, async (
                Guid variantId,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(variantId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RevokeVariantOptionValues))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Variants.OptionValues.Revoke.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.OptionValues.Revoke.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.OptionValues.Revoke.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
