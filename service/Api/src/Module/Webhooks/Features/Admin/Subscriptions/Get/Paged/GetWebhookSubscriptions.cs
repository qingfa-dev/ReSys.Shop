using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Features.Admin.Subscriptions.Get.Paged;

public static partial class GetWebhookSubscriptions
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parsing = request.Parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

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
