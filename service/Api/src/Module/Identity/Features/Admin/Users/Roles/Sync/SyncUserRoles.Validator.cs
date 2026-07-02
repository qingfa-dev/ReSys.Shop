using Module.Identity.Features.Admin.Users.Shared.Validators;

namespace Module.Identity.Features.Admin.Users.Roles.Sync;

public static partial class SyncUserRoles
{
    // ============ COMMAND VALIDATOR ============
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Request.Roles).ApplyRoleCollectionRules();
        }
    }
}
