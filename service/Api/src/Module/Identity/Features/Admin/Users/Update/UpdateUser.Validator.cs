using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Update;

public static partial class UpdateUser
{
    /// <summary>
    /// Validator for the <see cref="Request"/> to update an existing user.
    /// Ensures that the user ID is provided and that all core fields are valid.
    /// </summary>
    public sealed class Validator : AbstractValidator<Request>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        public Validator()
        {
            // Apply: Core field validation rules (Email, Username, Names).
            RuleFor(x => x.Email).ApplyUserEmailRules();
            RuleFor(x => x.UserName).ApplyUsernameRules();
            RuleFor(x => x.FirstName).ApplyUserFirstNameRules();
            RuleFor(x => x.LastName).ApplyUserLastNameRules(isRequired: false);
            RuleFor(x => x.PhoneNumber).ApplyUserPhoneRules()
                .When(m => !string.IsNullOrEmpty(m.PhoneNumber));

            // Note: Password is not validated here as it's typically handled via a separate change-password endpoint.
        }
    }
}