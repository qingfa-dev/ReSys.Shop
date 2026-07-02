using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Emails.Confirm;

public static partial class ConfirmEmail
{
    /// <summary>
    /// Validates ConfirmEmail command — ensures token follows user token rules.
    /// </summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Token).ApplyUserTokenRules();
        }
    }
}
