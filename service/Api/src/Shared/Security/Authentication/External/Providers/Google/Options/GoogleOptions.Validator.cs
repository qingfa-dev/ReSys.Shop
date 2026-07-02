using FluentValidation;

namespace Shared.Security.Authentication.External.Providers.Google.Options;

public sealed class GoogleOptionsValidator : AbstractValidator<GoogleOptions>
{
    public GoogleOptionsValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithErrorCode(GoogleOptionsResult.Failure.ClientIdRequired.Code)
            .WithMessage(GoogleOptionsResult.Failure.ClientIdRequired.Message);
    }
}
