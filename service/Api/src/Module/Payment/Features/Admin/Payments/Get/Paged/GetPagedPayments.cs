using Module.Payment.Features.Admin.Payments.Shared.Mappings;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Features.Admin.Payments.Get.Paged;

public static partial class GetPagedPayments
{
    public record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Parse: Query parameters — validates filters, sorting, pagination
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            // Load: Paged payment captures with sorting/filtering applied
            var pagedResult = await dbContext.Set<PaymentCapture>()
                .AsNoTracking()
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
