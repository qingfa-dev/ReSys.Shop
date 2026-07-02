using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Store.Auth.Logout;

public static partial class Logout
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.RefreshToken!).ApplyUserTokenRules()
                .When(x => !string.IsNullOrEmpty(x.RefreshToken));
        }
    }
}