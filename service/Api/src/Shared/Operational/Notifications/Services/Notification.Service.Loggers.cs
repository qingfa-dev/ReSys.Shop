using Shared.Operational.Notifications.Templates;

namespace Shared.Operational.Notifications.Services;

public partial class NotificationService
{
    /// <summary>Structured log event definitions for the notification orchestration service. Uses source-generated LoggerMessage for high-performance logging.</summary>
    internal static partial class Loggers
    {
        /// <summary>Logs that a notification job was enqueued for processing.</summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="useCase">The notification use case.</param>
        /// <param name="priority">The priority level of the notification.</param>
        /// <param name="queue">The queue name the job was enqueued to.</param>
        [LoggerMessage(
            EventId = 253,
            Level = LogLevel.Information,
            Message = "Enqueuing notification job. UseCase: {UseCase}, Priority: {Priority}, Queue: {Queue}")]
        public static partial void LogEnqueuingNotificationJob(ILogger logger, NotificationUseCase useCase, NotificationPriorityLevel priority, string queue);

        /// <summary>Logs that a notification delivery is being processed.</summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="useCase">The notification use case.</param>
        /// <param name="recipient">The recipient identifier.</param>
        [LoggerMessage(
            EventId = 254,
            Level = LogLevel.Information,
            Message = "Processing notification delivery. UseCase: {UseCase}, Recipient: {Recipient}")]
        public static partial void LogProcessingNotificationDelivery(ILogger logger, NotificationUseCase useCase, string recipient);

        /// <summary>Logs that a notification was handed off to the delivery pipeline.</summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="useCase">The notification use case.</param>
        /// <param name="recipient">The recipient identifier.</param>
        [LoggerMessage(
            EventId = 255,
            Level = LogLevel.Debug,
            Message = "Notification handed off to delivery pipeline. UseCase: {UseCase}, Recipient: {Recipient}")]
        public static partial void LogHandoffToDelivery(ILogger logger, NotificationUseCase useCase, string recipient);

        /// <summary>Logs that a background job was created for notification dispatch.</summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="jobId">The unique identifier of the background job.</param>
        /// <param name="useCase">The notification use case.</param>
        [LoggerMessage(
            EventId = 256,
            Level = LogLevel.Information,
            Message = "Background job created for notification dispatch. JobId: {JobId}, UseCase: {UseCase}")]
        public static partial void LogBackgroundJobCreated(ILogger logger, string jobId, NotificationUseCase useCase);

        /// <summary>Logs that template resolution failed for a given use case.</summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="useCase">The notification use case.</param>
        /// <param name="templateName">The name of the template that was not found.</param>
        [LoggerMessage(
            EventId = 257,
            Level = LogLevel.Warning,
            Message = "Template not found for notification. UseCase: {UseCase}, TemplateName: {TemplateName}")]
        public static partial void LogTemplateNotFound(ILogger logger, NotificationUseCase useCase, string templateName);

        /// <summary>Logs an unexpected exception during notification processing.</summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="useCase">The notification use case.</param>
        [LoggerMessage(
            EventId = 258,
            Level = LogLevel.Error,
            Message = "Unexpected exception during notification processing. UseCase: {UseCase}")]
        public static partial void LogServiceException(ILogger logger, Exception exception, NotificationUseCase useCase);
    }
}
