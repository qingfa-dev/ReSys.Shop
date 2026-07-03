using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Revoke;

public static partial class RevokeProductOptionTypes
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.OptionTypes.Revoke.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RevokeProductOptionTypes))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.OptionTypes.Revoke.Permission)
            .WithSummary(CatalogFeature.Admin.Products.OptionTypes.Revoke.Summary)
            .WithDescription(CatalogFeature.Admin.Products.OptionTypes.Revoke.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
