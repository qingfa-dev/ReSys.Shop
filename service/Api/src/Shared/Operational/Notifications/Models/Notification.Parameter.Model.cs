using Shared.Operational.Notifications.Templates;

namespace Shared.Operational.Notifications.Models;

/// <summary>
/// Represents a key-value pair used to populate placeholders in a notification template.
/// </summary>
/// <param name="Key">The parameter type.</param>
/// <param name="Value">The string value to inject.</param>
public sealed record NotificationParameter(NotificationParameterType Key, string? Value, bool IsRequired = true)
{
    /// <summary>
    /// Creates a new instance of <see cref="NotificationParameter"/>.
    /// </summary>
    /// <param name="key">The parameter type.</param>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="NotificationParameter"/>.</returns>
    public static NotificationParameter Create(
        NotificationParameterType key,
        string? value,
        bool isRequired = true) => new(key, value, isRequired);
}