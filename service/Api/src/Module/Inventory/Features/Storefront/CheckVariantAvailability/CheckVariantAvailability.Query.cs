using Shared.Application.Mediators.Queries;

namespace Module.Inventory.Features.Storefront.CheckVariantAvailability;

public sealed record CheckVariantAvailabilityQuery(Guid VariantId, int Quantity) : IQuery<CheckVariantAvailabilityResponse>;
