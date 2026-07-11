using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Features.Admin.Subscriptions.Get.Paged;

/// <summary>Retrieves a paged list of webhook subscriptions ordered by creation date.</summary>
public static partial class GetWebhookSubscriptions
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Parses query parameters, loads and paginates webhook subscriptions.</summary>
        /// <param name="request">The query containing paging and filtering parameters.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A paged result of subscription list items.</returns>
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=paged result returned
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            // Load: Subscriptions ordered by creation date
            var pagedResult = await dbContext.Set<WebhookSubscription>()
                .AsNoTracking()
                .OrderBy(s => s.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, s => new Response
                {
                    Id = s.Id,
                    Event = s.Event,
                    Url = s.Url,
                    Active = s.Active,
                    MaxRetries = s.MaxRetries,
                    CreatedAtUtc = s.CreatedAtUtc,
                }, cancellationToken);

            return pagedResult;
        }
    }
}
