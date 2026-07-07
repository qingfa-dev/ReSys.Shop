using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Features.Storefront.Payment.Methods;
    /// <summary>Handles ListPaymentMethods feature.</summary>
    public static partial class ListPaymentMethods
{
    public class Response { public Guid Id { get; init; } public string Name { get; init; } = null!; public string? Description { get; init; } }

    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext) : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Handles the query.</summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the query.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await dbContext.Set<PaymentMethod>().AsNoTracking()
                .Where(m => m.Active && !m.IsDeleted)
                .ApplyQuerying(parsing.Value)
                .Select(m => new Response { Id = m.Id, Name = m.Name, Description = m.Description })
                .ToPagedOrAllAsync(parsing.Value, x => x, cancellationToken);

            return pagedResult;
        }
    }
}
