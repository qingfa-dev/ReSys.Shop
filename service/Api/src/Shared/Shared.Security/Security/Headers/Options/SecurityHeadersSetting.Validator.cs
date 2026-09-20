using FluentValidation;

namespace Shared.Security.Headers.Options;

public sealed class SecurityHeadersSettingValidator : AbstractValidator<SecurityHeadersSetting>
{
    public SecurityHeadersSettingValidator()
    {
        RuleFor(s => s.ContentSecurityPolicy)
            .NotEmpty()
            .WithMessage("SecurityHeaders:ContentSecurityPolicy is required.");
        RuleFor(s => s.XFrameOptions)
            .NotEmpty()
            .WithMessage("SecurityHeaders:XFrameOptions is required.");
    }
}
