using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Auth.Sessions.Refresh;

public static partial class RefreshSession
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.RefreshToken).ApplyUserTokenRules();
        }
    }
}