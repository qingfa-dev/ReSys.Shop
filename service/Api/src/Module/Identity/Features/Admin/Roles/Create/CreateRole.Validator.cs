using Module.Identity.Features.Shared.Admin.Roles.Shared.Validators;

namespace Module.Identity.Features.Shared.Admin.Roles.Create;

public static partial class CreateRole
{
    /// <summary>
    /// Validator for the <see cref="Request"/> to create a new role.
    /// Ensures that the role name and description adhere to defined rules.
    /// </summary>
    public class Validator : AbstractValidator<Command>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        public Validator()
        {
            // Apply: Applies the common role validation rules for Name and Description.
            RuleFor(m => m.Request)
                .ApplyRoleParameterRules();
        }
    }
}