namespace Module.Webhooks.Features.Admin.Subscriptions.Update;

public static partial class UpdateWebhookSubscription
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(WebhooksFeature.Admin.Subscriptions.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .HasPermission(WebhooksFeature.Admin.Subscriptions.Update.Permission)
            .WithName(nameof(UpdateWebhookSubscription))
            .WithTags(WebhooksFeature.Tags.Subscription)
            .WithSummary(WebhooksFeature.Admin.Subscriptions.Update.Summary)
            .WithDescription(WebhooksFeature.Admin.Subscriptions.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}
