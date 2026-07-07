using Module.Webhooks.Domain;
using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Features.Admin.Subscriptions.Get.ById;

public static partial class GetWebhookSubscriptionById
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var subscription = await dbContext.Set<WebhookSubscription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == query.Id, cancellationToken);

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
