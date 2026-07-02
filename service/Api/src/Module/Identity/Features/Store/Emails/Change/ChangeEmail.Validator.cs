using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Emails.Change;

public static partial class ChangeEmail
{
    /// <summary>
    /// Validates ChangeEmail command — ensures new email and password follow user rules.
    /// </summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.NewEmail).ApplyUserEmailRules();
            RuleFor(x => x.Request.Password).ApplyUserPasswordRules();
        }
    }
}
