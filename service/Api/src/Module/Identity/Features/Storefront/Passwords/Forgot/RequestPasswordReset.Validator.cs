using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Storefront.Passwords.Forgot;

public static partial class RequestPasswordReset
{
    // ============ VALIDATOR ============
    /// <summary>
    /// Validates RequestPasswordReset request — ensures email follows user email rules.
    /// </summary>
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Email).ApplyUserEmailRules();
        }
    }
}