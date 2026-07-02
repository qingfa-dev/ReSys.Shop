using System.Text.RegularExpressions;

using FluentValidation;

using Shared.Operational.Notifications.Options;

namespace Shared.Operational.Notifications.Channels.Sms.Options;

/// <summary>Validates SMS channel configuration settings.</summary>
public sealed class SmsChannelSettingValidator : AbstractValidator<SmsChannelSetting>
{
    public SmsChannelSettingValidator()
    {
        // Validate: Ensure default sender number is provided
        RuleFor(x => x.DefaultSenderNumber)
            .NotEmpty()
            .WithErrorCode(SmsChannelSettingResult.Failure.DefaultSenderNumberRequired.Code)
            .WithMessage(SmsChannelSettingResult.Failure.DefaultSenderNumberRequired.Message);

        // Validate: Verify sender number matches phone format
        RuleFor(x => x.DefaultSenderNumber)
            .Matches(
                new Regex(
                    NotificationSettingConstant.Patterns.PhoneNumber,
                    RegexOptions.None,
                    TimeSpan.FromMilliseconds(100)))
            .When(x => !string.IsNullOrEmpty(x.DefaultSenderNumber))
            .WithErrorCode(SmsChannelSettingResult.Failure.DefaultSenderNumberInvalid.Code)
            .WithMessage(SmsChannelSettingResult.Failure.DefaultSenderNumberInvalid.Message);

        // Validate: Enforce maximum sender number length
        RuleFor(x => x.DefaultSenderNumber)
            .MaximumLength(SmsChannelSettingConstant.Constraints.DefaultSenderNumberMaxLength)
            .When(x => !string.IsNullOrEmpty(x.DefaultSenderNumber))
            .WithErrorCode(SmsChannelSettingResult.Failure.DefaultSenderNumberInvalid.Code)
            .WithMessage(SmsChannelSettingResult.Failure.DefaultSenderNumberInvalid.Message);
    }
}
