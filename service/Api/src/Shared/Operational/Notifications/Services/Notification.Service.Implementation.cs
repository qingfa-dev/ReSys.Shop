using Hangfire;
using Hangfire.States;

using Microsoft.Extensions.Options;

using Shared.Operational.Notifications.Hubs;
using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Store;
using Shared.Operational.Notifications.Templates;

namespace Shared.Operational.Notifications.Services;

/// <summary>
/// The primary orchestration service for the notification system.
/// It resolves templates, applies defaults, maps content, and dispatches to the unified <see cref="INotificationHub"/>.
/// </summary>
public sealed partial class NotificationService(
    INotificationHub notificationHub,
    IBackgroundJobClient? jobClient,
    IOptions<NotificationSetting> options,
    ILogger<NotificationService> logger)
    : INotificationService
{
    /// <inheritdoc />
    public async Task<Result> SendAsync(NotificationMessage message, CancellationToken ct = default)
    {
        // Check: Verify that a template exists for the requested use case
        if (!NotificationStore.Templates.TryGetValue(message.UseCase, out NotificationTemplate? template))
        {
            return NotificationResult.Failure.TemplateNotFound(message.UseCase.ToString());
        }

        // Call: Dispatch the notification via background job or execute synchronously
        if (options.Value.EnableBackgroundJobs)
        {
            if (jobClient is null)
            {
                return NotificationResult.Failure.BackgroundJobClientNotConfigured(message.UseCase.ToString());
            }

            var queue = template.Priority.ToQueueName();

            // Log: Record the hand-off to the background worker system
            Loggers.LogEnqueuingNotificationJob(logger, message.UseCase, template.Priority, queue);

            // Trigger: Create background job in the appropriate priority queue
            jobClient.Create<INotificationService>(
                service => service.SendInternalAsync(message, ct),
                new EnqueuedState(queue));

            return Result.Ok();
        }

        return await SendInternalAsync(message, ct);
    }

    /// <inheritdoc />
    public async Task<Result> SendInternalAsync(NotificationMessage message, CancellationToken ct = default)
    {
        // Transform: Fill missing system parameters with global defaults
        message = message.ApplyDefaults(options.Value);

        // Check: Final validation of template existence during delivery phase
        if (!NotificationStore.Templates.TryGetValue(message.UseCase, out NotificationTemplate? template))
        {
            return NotificationResult.Failure.TemplateNotFound(message.UseCase.ToString());
        }

        // Log: Trace the start of actual delivery processing
        Loggers.LogProcessingNotificationDelivery(logger, message.UseCase, message.Recipient.Identifier);

        // Transform: Render template placeholders with provided context values
        Result<NotificationContent> contentResult = message.MapContent();
        if (contentResult.IsFailure) return contentResult.Errors;

        // Call: Delegate fully-prepared message to the unified notification hub
        return await notificationHub.SendAsync(message, ct);
    }
}
