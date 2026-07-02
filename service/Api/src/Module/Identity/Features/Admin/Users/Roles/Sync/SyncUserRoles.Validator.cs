using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Users.Roles.Sync;

public static partial class SyncUserRoles
{
    // ============ COMMAND VALIDATOR ============
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleForEach(x => x.Request.Roles).ApplyRoleNameRules();
        }
    }
}
