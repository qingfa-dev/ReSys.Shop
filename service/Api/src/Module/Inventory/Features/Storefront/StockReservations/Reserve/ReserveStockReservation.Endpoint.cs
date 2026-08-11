using Carter;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Shared;
using Module.Inventory.Services.StockReservations;

namespace Module.Inventory.Features.Storefront.StockReservations.Reserve;

public static partial class ReserveStockReservation
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Storefront.StockReservations.Reserve.Route, async (
                [FromBody] Request request,
                IStockReservationService reservationService,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Items["CartToken"]?.ToString();
                var result = await reservationService.ReserveAsync(
                    request.VariantId, request.Quantity, request.StockLocationId,
                    cartToken: cartToken, ttlMinutes: request.TtlMinutes, ct: ct);
                return result.ToResult();
            })
            .WithName(nameof(ReserveStockReservation))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.StockReservations.Reserve.Summary)
            .WithDescription(InventoryFeature.Storefront.StockReservations.Reserve.Description)
            .Produces<Result<StockReservation>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
