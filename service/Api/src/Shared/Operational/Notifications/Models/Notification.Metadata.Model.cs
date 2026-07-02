using Shared.Governance.Conventions;

namespace Shared.Operational.Notifications.Models;

/// <summary>
/// Encapsulates non-content delivery metadata for a notification request.
/// </summary>
public static class NotificationMetadata
{
    /// <summary>Gets the priority level of the notification.</summary>
    public static readonly string Priority = nameof(Priority).ToCamelCase();

    /// <summary>Gets the preferred language for the content (e.g., "en-US").</summary>
    public static readonly string Language = nameof(Language).ToCamelCase();

    /// <summary>Gets the name of the system or user that initiated the notification.</summary>
    public static readonly string CreatedBy = nameof(CreatedBy).ToCamelCase();

}
