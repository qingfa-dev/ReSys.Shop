using Module.Webhooks.Features.Shared;

namespace Module.Webhooks.Features.Admin.Subscriptions.Get.ById;

public static partial class GetWebhookSubscriptionById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(WebhooksFeature.Admin.Subscriptions.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetWebhookSubscriptionById))
            .WithTags(WebhooksFeature.Tags.Webhook)
            .WithSummary(WebhooksFeature.Admin.Subscriptions.GetById.Summary)
            .WithDescription(WebhooksFeature.Admin.Subscriptions.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
