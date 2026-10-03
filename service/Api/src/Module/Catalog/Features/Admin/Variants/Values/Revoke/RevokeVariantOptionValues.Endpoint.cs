using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Variants.Values.Revoke;

public static partial class RevokeVariantOptionValues
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.VariantOptionValues.Revoke.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.VariantId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RevokeVariantOptionValues))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.VariantOptionValues.Revoke.Permission)
            .WithSummary(CatalogFeature.Admin.VariantOptionValues.Revoke.Summary)
            .WithDescription(CatalogFeature.Admin.VariantOptionValues.Revoke.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}