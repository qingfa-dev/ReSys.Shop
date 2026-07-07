using FluentValidation;

namespace Shared.Security.Authentication.External.Providers.Microsoft.Options;

public sealed class MicrosoftOptionsValidator : AbstractValidator<MicrosoftOptions>
{
    public MicrosoftOptionsValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithErrorCode(MicrosoftOptionsResult.Failure.ClientIdRequired.Code)
            .WithMessage(MicrosoftOptionsResult.Failure.ClientIdRequired.Message);
    }
}
