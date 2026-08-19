using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.Shared.Mappings;

namespace Module.Shipping.Features.Admin.ShippingMethods.Get.ById;

/// <summary>Retrieves a shipping method by its unique identifier.</summary>
public static partial class GetShippingMethodById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads a single shipping method by ID using a no-tracking query.</summary>
        /// <param name="request">The query containing the shipping method ID.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the shipping method details or a not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=method found or NotFound returned
            // Load: Shipping method by ID
            var method = await dbContext.Set<ShippingMethod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (method is null)
                return ShippingMethodResult.Errors.NotFound;

            return method.MapToDetail<Response>();
        }
    }
}