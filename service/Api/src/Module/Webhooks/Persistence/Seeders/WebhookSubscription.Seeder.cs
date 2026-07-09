using Shared.Operational.Persistence.Seeders;
using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Persistence.Seeders;

// TEMP: Dev-only seeder for testing webhook delivery flow — remove before production
public sealed class WebhookSubscriptionSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 110;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasSubscriptions = await HasDataAsync<WebhookSubscription>(cancellationToken);
        if (hasSubscriptions) return Result.Ok();

        var subscriptionResult = WebhookSubscriptionMethod.Create(
            @event: "order.placed",
            url: "http://localhost:9999/webhook",
            secretHash: "F6CEBF03E59AA8DBC5AEADCECC095ABD662B7AD30AF1FF07757B5690D742B9F4");
        if (subscriptionResult.IsFailure) return Result.Failure(subscriptionResult.Errors[0]);
        var subscription = subscriptionResult.Value;

        Context.Set<WebhookSubscription>().Add(subscription);
        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
