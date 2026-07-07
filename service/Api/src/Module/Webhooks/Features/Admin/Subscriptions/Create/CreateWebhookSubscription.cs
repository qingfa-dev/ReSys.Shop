using System.Security.Cryptography;
using System.Text;

using Shared.Operational.Webhooks.Domain;

namespace Module.Webhooks.Features.Admin.Subscriptions.Create;

public static partial class CreateWebhookSubscription
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var secretHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(request.Secret)));

            var subscription = new WebhookSubscription
            {
                Event = request.Event,
                Url = request.Url,
                SecretHash = secretHash,
                Active = true,
                HeadersJson = request.HeadersJson,
                MaxRetries = request.MaxRetries,
                CreatedAtUtc = DateTimeOffset.UtcNow,
            };

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
