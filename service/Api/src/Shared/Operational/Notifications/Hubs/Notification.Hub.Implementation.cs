using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Providers;

namespace Shared.Operational.Notifications.Hubs;

/// <summary>Orchestrates delivery across multiple INotificationProvider implementations with priority-based fallback.</summary>
public sealed partial class NotificationHub(
    IEnumerable<INotificationProvider> providers,
    ILogger<NotificationHub> logger)
    : INotificationHub
{
    public async Task<Result> SendAsync(
        NotificationMessage message,
        CancellationToken ct = default)
    {
        // Filter: Active providers matching the message delivery channel, ordered by priority
        var activeProviders = providers
            .Where(p => p.IsEnabled)
            .Where(p => p.Channel == message.Channel)
            .OrderBy(p => p.Priority)
            .ToList();

        if (activeProviders.Count == 0)
        {
            // Log: No active providers configured for this channel
            Loggers.LogNoActiveProviders(logger, message.Channel);
            return NotificationHubResult.Failure.NoProvidersConfigured(
                    message.Channel);
        }

        var notificationRecipient = message.Recipient.Name ?? message.Recipient.Identifier;
        // Fallback: Try each provider sequentially until one succeeds
        foreach (INotificationProvider? provider in activeProviders)
        {
            // Log: Attempting provider delivery
            Loggers.LogAttemptingToSend(
                logger,
                provider.Name,
                provider.Channel,
                provider.Priority,
                notificationRecipient);

            Result result = await provider.SendAsync(message, ct);

            // Check: Delivery succeeded — return immediately
            if (result.IsSuccess)
            {
                Loggers.LogSendSuccess(
                    logger,
                    provider.Name,
                    provider.Channel,
                    notificationRecipient);

                return Result.Ok();
            }

            // Log: Provider failed, try next in fallback chain
            Loggers.LogProviderFailed(
                logger,
                provider.Name,
                provider.Channel,
                result.Message);
        }

        // Log: All providers exhausted for this channel
        Loggers.LogAllProvidersFailed(
            logger,
            message.Channel,
            notificationRecipient);

        return
            NotificationHubResult.Failure.AllProvidersFailed(
                message.Channel);
    }
}