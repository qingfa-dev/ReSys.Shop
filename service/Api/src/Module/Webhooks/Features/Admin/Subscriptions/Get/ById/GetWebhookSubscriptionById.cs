using Module.Webhooks.Domain;
using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Features.Admin.Subscriptions.Get.ById;

/// <summary>Retrieves a webhook subscription by its unique identifier.</summary>
public static partial class GetWebhookSubscriptionById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        /// <summary>Loads a single subscription by ID using a no-tracking query.</summary>
        /// <param name="request">The query containing the subscription ID.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the subscription details or a not-found error.</returns>
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Contract: pre=request!=null, post=subscription found or NotFound returned
            // Load: Subscription by ID
            var subscription = await dbContext.Set<WebhookSubscription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (subscription is null)
                return WebhookSubscriptionResult.Errors.NotFound;

            return Result<Response>.Ok(new Response
            {
                Id = subscription.Id,
                Event = subscription.Event,
                Url = subscription.Url,
                Active = subscription.Active,
                MaxRetries = subscription.MaxRetries,
                HeadersJson = subscription.HeadersJson,
                CreatedAtUtc = subscription.CreatedAtUtc,
                ModifiedAtUtc = subscription.ModifiedAtUtc,
            });
        }
    }
}
