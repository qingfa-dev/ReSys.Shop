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
        });
    }
}
