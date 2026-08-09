using Module.Identity.Features.Shared.Admin.Users.Shared.Validators;

namespace Module.Identity.Features.Shared.Admin.Users.Create;

public static partial class CreateUser
{
    /// <summary>
    /// Validator for the <see cref="Request"/> to create a new user.
    /// Ensures that all user-related properties adhere to defined validation rules.
    /// </summary>
    public sealed class Validator : AbstractValidator<Request>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        public Validator()
        {
            // Apply: Common user validation rules.
            this.ApplyUserRules();
        }
    }
}