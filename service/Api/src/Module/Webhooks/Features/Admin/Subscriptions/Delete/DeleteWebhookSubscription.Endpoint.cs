using Module.Webhooks.Features.Shared;

namespace Module.Webhooks.Features.Admin.Subscriptions.Delete;

public static partial class DeleteWebhookSubscription
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(WebhooksFeature.Admin.Subscriptions.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteWebhookSubscription))
            .WithTags(WebhooksFeature.Tags.Webhook)
            .WithSummary(WebhooksFeature.Admin.Subscriptions.Delete.Summary)
            .WithDescription(WebhooksFeature.Admin.Subscriptions.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
