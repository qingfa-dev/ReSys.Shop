using Module.Identity.Features.Admin.Shared.Models;
using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Features.Admin.Shared.Validators;

/// <summary>
/// Provides a comprehensive extension method to apply all standard validation rules for role-related objects.
/// </summary>
public static partial class RoleValidator
{

    public sealed class RoleParameterValidator : AbstractValidator<RoleParameter>
    {
        public RoleParameterValidator()
        {
            // Apply: Name validation rules to the 'Name' property.
            RuleFor(x => x.Name).ApplyRoleNameRules();
            // Apply: Description validation rules to the 'Description' property.
            RuleFor(x => x.Description).ApplyRoleDescriptionRules();
        }
    }

    public static IRuleBuilderOptions<T, RoleParameter> ApplyRoleParameterRules<T>(
       this IRuleBuilder<T, RoleParameter> ruleBuilder)
    {
        return ruleBuilder
           .NotNull()
           .SetValidator(new RoleParameterValidator());
    }
}
