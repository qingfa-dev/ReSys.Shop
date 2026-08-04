using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Storefront.Emails.Resend;

public static partial class ResendEmailVerification
{
    /// <summary>
    /// Validates ResendEmailVerification command — ensures email follows user email rules.
    /// </summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Email).ApplyUserEmailRules();
        }
    }
}