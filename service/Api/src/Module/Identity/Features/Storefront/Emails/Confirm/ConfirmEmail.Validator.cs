using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Storefront.Emails.Confirm;

public static partial class ConfirmEmail
{
    /// <summary>
    /// Validates ConfirmEmail command — ensures token follows user token rules.
    /// </summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.UserId).NotEmpty();
            RuleFor(x => x.Request.Token).ApplyUserTokenRules();
        }
    }
}