using FluentValidation;

using Shared.Operational.Notifications.Options.Extensions;

namespace Shared.Operational.Notifications.Channels.Emails.Options;

/// <summary>
/// Fluent validator for <see cref="EmailOptions"/>.
/// </summary>
public sealed class EmailChannelSettingValidator : AbstractValidator<EmailChannelSetting>
{
    public EmailChannelSettingValidator()
    {
        RuleFor(x => x.FromEmail)
        .MustBeValidEmail()
        .WithErrorCode(EmailChannelSettingResult.Failure.FromEmailRequired.Code)
        .WithMessage(EmailChannelSettingResult.Failure.FromEmailRequired.Message);

        RuleFor(x => x.FromName)
        .NotEmpty()
        .WithErrorCode(EmailChannelSettingResult.Failure.FromNameRequired.Code)
        .WithMessage(EmailChannelSettingResult.Failure.FromNameRequired.Message);
    }
}
