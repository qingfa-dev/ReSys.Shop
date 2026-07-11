using Module.Webhooks.Domain;
using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Features.Admin.Subscriptions.Delete;

/// <summary>Hard-deletes a webhook subscription.</summary>
public static partial class DeleteWebhookSubscription
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Finds the subscription by ID and removes it from the database.</summary>
        /// <param name="command">The command identifying the subscription to delete.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A success result or a not-found error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=subscription!=null, post=subscription removed, throws=DbUpdateException
            // Load: Find the subscription by ID
            var subscription = await dbContext.Set<WebhookSubscription>()
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (subscription is null)
                return WebhookSubscriptionResult.Errors.NotFound;

            // Remove: Delete the subscription from the database
            dbContext.Set<WebhookSubscription>().Remove(subscription);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
