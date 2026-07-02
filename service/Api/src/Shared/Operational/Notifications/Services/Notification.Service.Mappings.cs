using System.Text.RegularExpressions;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Store;
using Shared.Operational.Notifications.Templates;

namespace Shared.Operational.Notifications.Services;

/// <summary>
/// Provides extension methods for mapping <see cref="NotificationMessage"/> instances to final <see cref="NotificationContent"/>.
/// </summary>
public static class NotificationMapper
{
    private static readonly Regex PlaceholderRegex = new(@"\{(\w+)\}", RegexOptions.Compiled);

    /// <summary>
    /// Maps a <see cref="NotificationMessage"/> to <see cref="NotificationContent"/> by filling template placeholders with context values.
    /// </summary>
    /// <param name="message">The notification request.</param>
    /// <returns>A <see cref="Result{T}"/> containing the rendered content or failures.</returns>
    public static Result<NotificationContent> MapContent(this NotificationMessage message)
    {
        // Check: Ensure the requested template is available in the store
        if (!NotificationStore.Templates.TryGetValue(message.UseCase, out NotificationTemplate? template))
        {
            return NotificationResult.Failure.TemplateNotFound(message.UseCase.ToString());
        }

        // Transform: Fill subject and body placeholders with context data
        var subject = FillTemplate(template.Name, message.Context);
        var body = FillTemplate(template.TemplateContent ?? string.Empty, message.Context);
        var htmlBody = FillTemplate(template.HtmlTemplateContent ?? string.Empty, message.Context);

        // Create: Instantiate the final rendered content object
        return NotificationContent.Create(subject, body, string.IsNullOrWhiteSpace(htmlBody) ? null : htmlBody);
    }

    /// <summary>
    /// Fills missing system parameters in the <see cref="NotificationMessage"/> context with default values from <see cref="NotificationSetting"/>.
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <param name="defaults">The default options.</param>
    /// <returns>A new message with updated context if changes were made; otherwise, the original message.</returns>
    public static NotificationMessage ApplyDefaults(this NotificationMessage message, NotificationSetting defaults)
    {
        NotificationContext context = message.Context;
        var hasChanges = false;

        // Transform: Iterate through system parameters and apply missing defaults
        foreach (NotificationParameterType paramType in GetAllSystemParameterTypes())
        {
            var currentValue = context.GetValue(paramType);
            if (string.IsNullOrEmpty(currentValue))
            {
                var defaultValue = paramType switch
                {
                    NotificationParameterType.ApplicationName => defaults.ApplicationName,
                    NotificationParameterType.SupportEmail => defaults.SupportEmail,
                    NotificationParameterType.SupportPhone => defaults.SupportPhone,
                    NotificationParameterType.ApplicationUrl => defaults.ApplicationUrl,
                    NotificationParameterType.UnsubscribeUrl => defaults.UnsubscribeUrl,
                    _ => null
                };

                if (!string.IsNullOrEmpty(defaultValue))
                {
                    // Merge: Add default value to the existing notification context
                    context = NotificationContext.ApplyParameter(context, paramType, defaultValue);
                    hasChanges = true;
                }
            }
        }

        return hasChanges ? message with { Context = context } : message;
    }

    /// <summary>
    /// Maps a <see cref="NotificationPriorityLevel"/> to its corresponding background job queue name.
    /// </summary>
    /// <param name="priority">The priority level.</param>
    /// <returns>The queue name.</returns>
    public static string ToQueueName(this NotificationPriorityLevel priority) => priority switch
    {
        // Map: Priority enums to standard background job queue strings
        NotificationPriorityLevel.Critical => NotificationSettingConstant.BackgroundJobs.Queues.Critical,
        NotificationPriorityLevel.High => NotificationSettingConstant.BackgroundJobs.Queues.High,
        NotificationPriorityLevel.Normal => NotificationSettingConstant.BackgroundJobs.Queues.Default,
        NotificationPriorityLevel.Low => NotificationSettingConstant.BackgroundJobs.Queues.Low,
        _ => NotificationSettingConstant.BackgroundJobs.Queues.Default
    };

    private static string FillTemplate(string template, NotificationContext context)
    {
        // Guard: Skip empty or whitespace templates
        if (string.IsNullOrWhiteSpace(template)) return template;

        // Transform: Replace {Placeholder} patterns with context values using regex
        return PlaceholderRegex.Replace(template, match =>
        {
            var keyName = match.Groups[1].Value;
            if (Enum.TryParse<NotificationParameterType>(keyName, out NotificationParameterType param))
            {
                // Fallback: Return the original placeholder if no value is found in context
                return context.GetValue(param) ?? match.Value;
            }
            return match.Value;
        });
    }

    private static IEnumerable<NotificationParameterType> GetAllSystemParameterTypes()
    {
        yield return NotificationParameterType.ApplicationName;
        yield return NotificationParameterType.ApplicationUrl;
        yield return NotificationParameterType.SupportEmail;
        yield return NotificationParameterType.SupportPhone;
        yield return NotificationParameterType.UnsubscribeUrl;
    }
}
