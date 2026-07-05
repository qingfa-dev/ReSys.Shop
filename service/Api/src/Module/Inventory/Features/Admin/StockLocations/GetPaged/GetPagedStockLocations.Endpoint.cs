using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockLocations.GetPaged;

public static partial class GetPagedStockLocations
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockLocations.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetPagedStockLocations))
            .WithTags(InventoryFeature.Tags.StockLocation)
            .HasPermission(InventoryFeature.Admin.StockLocations.GetAll.Permission)
            .WithSummary(InventoryFeature.Admin.StockLocations.GetAll.Summary)
            .WithDescription(InventoryFeature.Admin.StockLocations.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}
