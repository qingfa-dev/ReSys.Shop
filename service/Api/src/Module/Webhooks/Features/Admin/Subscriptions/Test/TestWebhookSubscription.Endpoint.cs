namespace Module.Webhooks.Features.Admin.Subscriptions.Test;

public static partial class TestWebhookSubscription
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(WebhooksFeature.Admin.Subscriptions.Test.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(TestWebhookSubscription))
            .WithTags(WebhooksFeature.Tags.Subscription)
            .WithSummary(WebhooksFeature.Admin.Subscriptions.Test.Summary)
            .WithDescription(WebhooksFeature.Admin.Subscriptions.Test.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
