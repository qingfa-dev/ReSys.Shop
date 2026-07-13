using Shared.Security.Identity.Domain.Roles;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Roles.Sync;

public static partial class SyncUserRoles
{
    // ============ COMMAND VALIDATOR ============
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithErrorCode(UserResult.Failure.IdRequired.Code)
                .WithMessage(UserResult.Failure.IdRequired.Message);

            RuleForEach(x => x.Request.Roles).ApplyRoleNameRules();
        }
    }
}