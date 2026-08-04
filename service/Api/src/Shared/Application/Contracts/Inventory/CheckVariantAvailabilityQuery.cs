using Shared.Application.Mediators.Queries;

namespace Shared.Application.Contracts.Inventory;

public sealed record CheckVariantAvailabilityQuery(Guid VariantId, int Quantity) : IQuery<CheckVariantAvailabilityResponse>;

public sealed record CheckVariantAvailabilityResponse
{
    public Guid VariantId { get; init; }
    public bool IsAvailable { get; init; }
}
