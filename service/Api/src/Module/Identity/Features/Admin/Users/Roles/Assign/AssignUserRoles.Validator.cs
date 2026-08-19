using Module.Identity.Features.Admin.Shared.Validators;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Shared.Admin.Users.Roles.Assign;

public static partial class AssignUserRoles
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

            RuleFor(x => x.Request.Roles).ApplyRoleCollectionRules();
        }
    }
}