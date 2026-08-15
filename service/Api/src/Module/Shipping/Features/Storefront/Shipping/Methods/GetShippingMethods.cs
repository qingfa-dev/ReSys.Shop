using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

namespace Module.Shipping.Features.Storefront.Shipping.Methods;
/// <summary>Retrieves all shipping methods available to storefront users.</summary>
public static partial class GetShippingMethods
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext, ILogger<PagedQueryHandler> logger)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads active, non-deleted shipping methods (optionally zone-filtered) and returns them as a list.</summary>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=none, post=list of available shipping methods returned
            _ = logger;
            // Load: Available shipping methods, optionally filtered by delivery country zone.
            var query = dbContext.Set<ShippingMethod>()
                .AsNoTracking()
                .Where(x => x.AvailableToUsers && !x.IsDeleted);

            if (!string.IsNullOrEmpty(request.Parameters.CountryCode))
            {
                var countryCode = request.Parameters.CountryCode.ToUpperInvariant();
                query = query.Where(m => m.Zones.Any(z =>
                    z.CountryCode == "*" || z.CountryCode == countryCode));
            }

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;

            // Map: Project to response DTO with DB-side paging.
            return await query
                .OrderBy(m => m.Position)
                .ToPagedOrAllAsync(
                    m => m.MapToDetail<Response>(),
                    pageModel,
                    cancellationToken);
        }
    }
}
