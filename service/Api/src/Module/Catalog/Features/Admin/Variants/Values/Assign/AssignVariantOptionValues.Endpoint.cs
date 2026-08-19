using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Variants.Values.Assign;

public static partial class AssignVariantOptionValues
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.VariantOptionValues.Assign.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.VariantId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(AssignVariantOptionValues))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.VariantOptionValues.Assign.Permission)
            .WithSummary(CatalogFeature.Admin.VariantOptionValues.Assign.Summary)
            .WithDescription(CatalogFeature.Admin.VariantOptionValues.Assign.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}