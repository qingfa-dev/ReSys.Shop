using FluentValidation;

namespace Shared.Security.Authentication.External.Providers.Facebook.Options;

public sealed class FacebookOptionsValidator : AbstractValidator<FacebookOptions>
{
    public FacebookOptionsValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithErrorCode(FacebookOptionsResult.Failure.ClientIdRequired.Code)
            .WithMessage(FacebookOptionsResult.Failure.ClientIdRequired.Message);
    }
}
