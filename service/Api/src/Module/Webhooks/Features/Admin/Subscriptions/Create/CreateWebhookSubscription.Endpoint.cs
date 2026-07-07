using Module.Webhooks.Features.Shared;

namespace Module.Webhooks.Features.Admin.Subscriptions.Create;

public static partial class CreateWebhookSubscription
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(WebhooksFeature.Admin.Subscriptions.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateWebhookSubscription))
            .WithTags(WebhooksFeature.Tags.Webhook)
            .WithSummary(WebhooksFeature.Admin.Subscriptions.Create.Summary)
            .WithDescription(WebhooksFeature.Admin.Subscriptions.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
