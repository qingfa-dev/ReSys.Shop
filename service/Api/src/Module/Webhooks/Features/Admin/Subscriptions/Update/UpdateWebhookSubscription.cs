using Module.Webhooks.Domain;
using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Features.Admin.Subscriptions.Update;

public static partial class UpdateWebhookSubscription
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var subscription = await dbContext.Set<WebhookSubscription>()
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (subscription is null)
                return WebhookSubscriptionResult.Errors.NotFound;

            var request = command.Request;

            if (request.Url is not null)
            {
                var urlValidation = WebhookUrlValidator.ValidateUrl(request.Url);
                if (urlValidation.IsFailure)
                    return urlValidation.Errors;

                subscription.Url = request.Url;
            }

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
