using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Features.Storefront.CheckVariantAvailability;

/// <summary>Lightweight reservation-aware stock check for a variant — tolerates slightly stale reads for UX pre-validation.</summary>
public sealed class CheckVariantAvailabilityQueryHandler(IStockAvailabilityService availabilityService)
    : IQueryHandler<CheckVariantAvailabilityQuery, CheckVariantAvailabilityResponse>
{
    /// <summary>Returns whether the requested quantity is available anywhere for the variant.</summary>
    public async Task<Result<CheckVariantAvailabilityResponse>> Handle(
        CheckVariantAvailabilityQuery query, CancellationToken cancellationToken)
    {
        if (query.Quantity <= 0)
            return new CheckVariantAvailabilityResponse
            {
                VariantId = query.VariantId,
                IsAvailable = true
            };

        var isAvailable = await availabilityService.IsAvailableAnyLocationAsync(
            query.VariantId, query.Quantity, cancellationToken);

        return new CheckVariantAvailabilityResponse
        {
            VariantId = query.VariantId,
            IsAvailable = isAvailable
        };
    }
}
