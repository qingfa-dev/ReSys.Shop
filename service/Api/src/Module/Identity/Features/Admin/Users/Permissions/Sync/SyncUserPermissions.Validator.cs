using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Admin.Users.Permissions.Sync;

public static partial class SyncUserPermissions
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

            RuleFor(x => x.Request.Permissions)
                .NotEmpty()
                .WithErrorCode("PermissionsRequired")
                .WithMessage("At least one permission must be specified.");
        }
    }
}