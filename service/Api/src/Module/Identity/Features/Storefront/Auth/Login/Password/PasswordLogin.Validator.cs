using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Storefront.Auth.Login.Password;

public static partial class PasswordLogin
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Credential)
                .ApplyUserCredentialRules();
            RuleFor(x => x.Request.Password)
                .ApplyUserPasswordRules(
                    requireMinLength: false,
                    requireStrongPassword: false);
        }
    }
}