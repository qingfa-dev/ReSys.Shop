using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;

/// <summary>Lightweight reservation-aware stock check for a variant — tolerates slightly stale reads for UX pre-validation.</summary>
public static partial class CheckStockAvailability
{
    public sealed record Query(Request Request) : IQuery<Response>;

    public sealed class QueryHandler(IStockAvailabilityService availabilityService)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var req = request.Request;

            if (req.Quantity <= 0)
                return new Response { VariantId = req.VariantId, IsAvailable = true };

            var isAvailable = await availabilityService.IsAvailableAnyLocationAsync(
                req.VariantId, req.Quantity, cancellationToken);

            return new Response
            {
                VariantId = req.VariantId,
                IsAvailable = isAvailable
            };
        }
    }
}
