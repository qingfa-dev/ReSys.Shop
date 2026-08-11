using Carter;
using Module.Inventory.Features.Shared;
using Module.Inventory.Services.StockReservations;

namespace Module.Inventory.Features.Storefront.StockReservations.Release;

public static partial class ReleaseStockReservation
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(InventoryFeature.Storefront.StockReservations.Release.Route, async (
                [FromRoute] Guid id,
                IStockReservationService reservationService,
                CancellationToken ct) =>
            {
                var result = await reservationService.ReleaseReservationAsync(id, ct);
                return result.ToResult();
            })
            .WithName(nameof(ReleaseStockReservation))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.StockReservations.Release.Summary)
            .WithDescription(InventoryFeature.Storefront.StockReservations.Release.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }

    public sealed class Validator : AbstractValidator<Guid>
    {
        public Validator() { RuleFor(x => x).NotEmpty(); }
    }
}
