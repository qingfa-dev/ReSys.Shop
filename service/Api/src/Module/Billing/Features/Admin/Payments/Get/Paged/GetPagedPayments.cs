using Module.Billing.Features.Admin.Shared.Mappings;

using Module.Billing.Domain.PaymentCaptures;


namespace Module.Billing.Features.Admin.Payments.Get.Paged;

/// <summary>Retrieves a paged list of payments.</summary>
public static partial class GetPagedPayments
{
    public record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Retrieves a paged list of payments.</summary>
        // Contract: pre=request!=null, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Parse: Query parameters — validates filters, sorting, pagination
            var parsing = request.Parameters.ParseAll(
                allowedFilterFields: PaymentConstant.Query.AllowedFilterFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSearchFields: PaymentConstant.Query.AllowedSearchFields.ToHashSet(StringComparer.OrdinalIgnoreCase),
                allowedSortFields: PaymentConstant.Query.AllowedSortFields.ToHashSet(StringComparer.OrdinalIgnoreCase));
            if (parsing.IsFailure)
                return parsing.Errors;

            // Load: Page of payment capture entities with payment method included (mapping reads the nav).
            var pagedResult = await dbContext.Set<PaymentCapture>()
                .AsNoTracking()
                .Include(p => p.PaymentMethod)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, cancellationToken);

            // Map: Convert each entity to a list item response in memory (method-call projection is not SQL-translatable).
            var items = pagedResult.Items
                .Select(p => p.MapToListItem<Response>())
                .ToList();

            return PagedResult<Response>.Create(items, pagedResult.PageNumber, pagedResult.PageSize, pagedResult.TotalCount);
        }
    }
}
