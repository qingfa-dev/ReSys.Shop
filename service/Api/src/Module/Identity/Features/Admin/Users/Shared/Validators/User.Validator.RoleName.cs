using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Users.Shared.Validators;

public static class UserRoleValidations
{
    public static IRuleBuilderOptions<T, IEnumerable<string>> ApplyRoleCollectionRules<T>(this IRuleBuilder<T, IEnumerable<string>> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode("Role.RolesRequired")
            .WithMessage("At least one role is required.")
            .ForEach(role => role.ApplyRoleNameRules());
    }
}