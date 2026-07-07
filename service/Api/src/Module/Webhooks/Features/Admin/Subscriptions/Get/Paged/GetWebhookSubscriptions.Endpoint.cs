using Module.Webhooks.Features.Shared;

namespace Module.Webhooks.Features.Admin.Subscriptions.Get.Paged;

public static partial class GetWebhookSubscriptions
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(WebhooksFeature.Admin.Subscriptions.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetWebhookSubscriptions))
            .WithTags(WebhooksFeature.Tags.Webhook)
            .WithSummary(WebhooksFeature.Admin.Subscriptions.GetAll.Summary)
            .WithDescription(WebhooksFeature.Admin.Subscriptions.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
