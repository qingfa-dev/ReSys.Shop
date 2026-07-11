using System.Security.Cryptography;
using System.Text;

using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Features.Admin.Subscriptions.Create;

/// <summary>Creates a new webhook subscription with URL validation and secret hashing.</summary>
public static partial class CreateWebhookSubscription
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates the webhook URL, hashes the secret, and persists the subscription.</summary>
        /// <param name="command">The command containing the webhook subscription data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the created subscription details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null && command.Request!=null && url valid,
            //           post=subscription persisted, throws=DbUpdateException
            var request = command.Request;

            // Validate: Webhook URL format and reachability
            var urlValidation = WebhookUrlValidator.ValidateUrl(request.Url);
            if (urlValidation.IsFailure)
                return urlValidation.Errors;

            // Generate: SHA-256 hash of the webhook secret for secure storage
            var secretHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(request.Secret)));

            // Create: Build webhook subscription domain entity
            var subscriptionResult = WebhookSubscriptionMethod.Create(
                @event: request.Event,
                url: request.Url,
                secretHash: secretHash,
                maxRetries: request.MaxRetries,
                headersJson: request.HeadersJson);
            if (subscriptionResult.IsFailure) return Result<Response>.Failure(subscriptionResult.Errors[0]);
            var subscription = subscriptionResult.Value;

            dbContext.Set<WebhookSubscription>().Add(subscription);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Created(new Response
            {
                Id = subscription.Id,
                Event = subscription.Event,
                Url = subscription.Url,
                Active = subscription.Active,
                MaxRetries = subscription.MaxRetries,
            });
        }
    }
}
