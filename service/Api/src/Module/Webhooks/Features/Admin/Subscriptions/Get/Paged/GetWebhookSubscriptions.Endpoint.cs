namespace Module.Webhooks.Features.Admin.Subscriptions.Get.Paged;

public static partial class GetWebhookSubscriptions
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(WebhooksFeature.Admin.Subscriptions.GetPaged.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .HasPermission(WebhooksFeature.Admin.Subscriptions.GetPaged.Permission)
            .WithName(nameof(GetWebhookSubscriptions))
            .WithTags(WebhooksFeature.Tags.Subscription)
            .WithSummary(WebhooksFeature.Admin.Subscriptions.GetPaged.Summary)
            .WithDescription(WebhooksFeature.Admin.Subscriptions.GetPaged.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
