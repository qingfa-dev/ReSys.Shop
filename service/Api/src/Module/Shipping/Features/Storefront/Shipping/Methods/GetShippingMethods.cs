using Module.Shipping.Domain.ShippingMethods;

namespace Module.Shipping.Features.Storefront.Shipping.Methods;
/// <summary>Retrieves all shipping methods available to storefront users.</summary>
public static partial class GetShippingMethods
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext, ILogger<PagedQueryHandler> logger)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Loads active, non-deleted shipping methods and returns them as a list.</summary>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=none, post=list of available shipping methods returned
            _ = logger;
            // Load: All available shipping methods.
            var methods = await dbContext.Set<ShippingMethod>()
                .Where(x => x.AvailableToUsers && !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map: Return list of available shipping methods.
            // EXCEPTION: no domain entity — maps from domain ShippingMethod entities to DTOs
            var items = methods.Select(m => new Response
            {
                Id = m.Id,
                Name = m.Name,
                AdminName = m.AdminName,
                Code = m.Code,
                CalculatorType = m.CalculatorType,
                Position = m.Position
            }).OrderBy(m => m.Position).ToList();

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count)
                : items.ToPagedResult(pageModel);
        }
    }
}
