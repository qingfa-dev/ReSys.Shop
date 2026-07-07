using Module.Webhooks.Domain;
using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Features.Admin.Subscriptions.Delete;

public static partial class DeleteWebhookSubscription
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var subscription = await dbContext.Set<WebhookSubscription>()
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (subscription is null)
                return WebhookSubscriptionResult.Errors.NotFound;

            dbContext.Set<WebhookSubscription>().Remove(subscription);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
