using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Passwords.Reset;

public static partial class ResetPassword
{
    /// <summary>
    /// Validates ResetPassword request — ensures token and new password follow user rules.
    /// </summary>
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Token).ApplyUserTokenRules();
            RuleFor(x => x.NewPassword).ApplyUserPasswordRules(requireMinLength: true);
        }
    }
}