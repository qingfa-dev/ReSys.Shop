using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Storefront.Shared.Mappings;

namespace Module.Shipping.Features.Storefront.ShippingMethods.Get;

public static partial class GetShippingMethods
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads active, non-deleted shipping methods (optionally zone-filtered) and returns them as a list.</summary>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=none, post=list of available shipping methods returned
            // Load: Available shipping methods, optionally filtered by delivery country zone.
            var parsing = request.Parameters.ParseAll(
                allowedFilterFields: ShippingMethodConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: ShippingMethodConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: ShippingMethodConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return parsing.Errors;

            var query = dbContext.Set<ShippingMethod>()
                .AsNoTracking()
                .Where(x => x.AvailableToUsers && !x.IsDeleted);

            if (!string.IsNullOrEmpty(request.Parameters.CountryCode))
            {
                var countryCode = request.Parameters.CountryCode.ToUpperInvariant();
                var zoneMethodIds = dbContext.Set<ShippingMethodZone>()
                    .Where(z => z.CountryCode == "*" || z.CountryCode == countryCode)
                    .Select(z => z.ShippingMethodId);
                query = query.Where(m => zoneMethodIds.Contains(m.Id));
            }

            var pagedResult = await query
                .OrderBy(m => m.Position)
                .ThenBy(m => m.Name)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, m => m.MapToStorefrontListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
