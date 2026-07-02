namespace Module.Identity.Features.Admin.Roles.Permissions.Sync;

public static partial class SyncRolePermissions
{
    // ============ COMMAND VALIDATOR ============
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Request.Permissions)
                .NotEmpty()
                .WithErrorCode("PermissionsRequired")
                .WithMessage("At least one permission must be specified.");
        }
    }
}
