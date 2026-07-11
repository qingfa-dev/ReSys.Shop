using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Roles.Permissions.Assign;

public static partial class AssignRolePermissions
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
