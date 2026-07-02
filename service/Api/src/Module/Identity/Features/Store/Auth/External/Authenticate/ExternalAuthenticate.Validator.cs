namespace Module.Identity.Features.Store.Auth.External.Authenticate;

public static partial class ExternalAuthenticate
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Provider)
                .NotEmpty()
                .WithErrorCode("ExternalLogin.ProviderRequired");

            RuleFor(x => x.Request.IdToken)
                .NotEmpty()
                .WithErrorCode("ExternalLogin.IdTokenRequired");

            RuleFor(x => x.Request.IdToken)
                .MaximumLength(65536)
                .WithErrorCode("ExternalLogin.IdTokenTooLong");
        }
    }
}
