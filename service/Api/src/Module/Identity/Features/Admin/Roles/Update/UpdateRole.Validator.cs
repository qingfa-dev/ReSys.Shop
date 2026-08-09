using Module.Identity.Features.Shared.Admin.Roles.Shared.Validators;

namespace Module.Identity.Features.Shared.Admin.Roles.Update;

public static partial class UpdateRole
{
    /// <summary>
    /// Validator for the <see cref="Command"/> to update an existing role.
    /// Ensures that the role ID is provided and that the role name and description adhere to defined rules.
    /// </summary>
    public class Validator : AbstractValidator<Command>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        public Validator()
        {
            // Apply: Apply common role validation rules for Name and Description.
            RuleFor(x => x.Request).ApplyRoleParameterRules();
        }
    }
}