using Module.Identity.Features.Admin.Users.Shared.Models;

using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Features.Admin.Users.Shared.Validators;

public static partial class UserValidator
{
    /// <summary>
    /// Applies a set of common validation rules for user parameters, including email, username, password, and name.
    /// </summary>
    /// <typeparam name="T">The type of the validator, constrained to <see cref="UserParameter"/>.</typeparam>
    /// <param name="validator">The abstract validator instance.</param>
    public static void ApplyUserRules<T>(this AbstractValidator<T> validator) where T : UserParameter
    {
        // Apply: Email validation rules.
        validator.RuleFor(x => x.Email).ApplyUserEmailRules();

        // Apply: Username validation rules.
        validator.RuleFor(x => x.UserName).ApplyUsernameRules();

        // Apply: First name validation rules.
        validator.RuleFor(x => x.FirstName).ApplyUserFirstNameRules();

        // Apply: Last name validation rules.
        validator.RuleFor(x => x.LastName).ApplyUserLastNameRules(isRequired: false);

        // Apply: Phone validation rules (optional).
        validator.RuleFor(x => x.PhoneNumber).ApplyUserPhoneRules()
            .When(m => !string.IsNullOrEmpty(m.PhoneNumber));
    }
}