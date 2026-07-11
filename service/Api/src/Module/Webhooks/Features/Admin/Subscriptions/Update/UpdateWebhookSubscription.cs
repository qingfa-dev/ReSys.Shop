using Module.Webhooks.Domain;
using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Features.Admin.Subscriptions.Update;

/// <summary>Updates an existing webhook subscription with PATCH semantics.</summary>
public static partial class UpdateWebhookSubscription
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Applies partial updates to URL, active flag, max retries, and headers — validates URL if provided.</summary>
        /// <param name="command">The command containing the subscription ID and update data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the updated subscription details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=subscription!=null, post=subscription updated, throws=DbUpdateException
            // Load: Find the subscription by ID
            var subscription = await dbContext.Set<WebhookSubscription>()
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (subscription is null)
                return WebhookSubscriptionResult.Errors.NotFound;

            var request = command.Request;

            // Validate: URL format if being updated
            if (request.Url is not null)
            {
                var urlValidation = WebhookUrlValidator.ValidateUrl(request.Url);
                if (urlValidation.IsFailure)
                    return urlValidation.Errors;

                subscription.Url = request.Url;
            }

            // Update: Apply field-level changes
            if (request.Active is not null)
                subscription.Active = request.Active.Value;

            if (request.MaxRetries is not null)
                subscription.MaxRetries = request.MaxRetries.Value;

            if (request.HeadersJson is not null)
                subscription.HeadersJson = request.HeadersJson;

            subscription.ModifiedAtUtc = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Ok(new Response
            {
                Id = subscription.Id,
                Event = subscription.Event,
                Url = subscription.Url,
                Active = subscription.Active,
                MaxRetries = subscription.MaxRetries,
                HeadersJson = subscription.HeadersJson,
                CreatedAtUtc = subscription.CreatedAtUtc,
                ModifiedAtUtc = subscription.ModifiedAtUtc,
            });
        }
    }
}
