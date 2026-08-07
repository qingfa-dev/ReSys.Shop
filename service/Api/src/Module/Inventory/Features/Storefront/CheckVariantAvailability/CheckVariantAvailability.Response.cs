namespace Module.Inventory.Features.Storefront.CheckVariantAvailability;

public sealed record CheckVariantAvailabilityResponse
{
    public Guid VariantId { get; init; }
    public bool IsAvailable { get; init; }
}
