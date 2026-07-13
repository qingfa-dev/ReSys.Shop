using Module.Identity.Features.Admin.Users.Shared.Validators;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Roles.Revoke;

public static partial class RevokeUserRoles
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