using Shared.Operational.Notifications.Options;
using Shared.Operational.Notifications.Options.Channels;

namespace Shared.Operational.Notifications.Channels.Sms.Options;

/// <summary>Configuration settings for the SMS notification channel.</summary>
public sealed class SmsChannelSetting : ChannelSettingBase
{
    /// <inheritdoc />
    public static new string Section => $"{NotificationSetting.Section}:Channels:Sms";

    /// <summary>Gets or sets the default sender phone number for SMS messages.</summary>
    public string DefaultSenderNumber { get; set; } = string.Empty;
}
