using FluentValidation;

namespace Shared.Security.Headers.Options;

public sealed class SecurityHeadersSettingValidator : AbstractValidator<SecurityHeadersSetting>
{
    public SecurityHeadersSettingValidator()
    {
        When(x => x.IsEnabled, () =>
        {
            RuleFor(x => x.XContentTypeOptions)
                .NotEmpty()
                .WithMessage("X-Content-Type-Options must not be empty when enabled.");

            RuleFor(x => x.XFrameOptions).NotEmpty().WithErrorCode("SecurityHeaders.XFrameOptions.Required");
            RuleFor(x => x.ReferrerPolicy).NotEmpty().WithErrorCode("SecurityHeaders.ReferrerPolicy.Required");
        });
    }
}
