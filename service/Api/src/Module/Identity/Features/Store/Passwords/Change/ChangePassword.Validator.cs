using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Passwords.Change;

public static partial class ChangePassword
{
    /// <summary>
    /// Validates ChangePassword request — ensures current password and new password follow user rules.
    /// </summary>
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.CurrentPassword).ApplyUserPasswordRules();
            RuleFor(x => x.NewPassword).ApplyUserPasswordRules(requireMinLength: true);
        }
    }
}
