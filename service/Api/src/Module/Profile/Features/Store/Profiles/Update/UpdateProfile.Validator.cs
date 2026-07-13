using Module.Profile.Domain;

namespace Module.Profile.Features.Store.Profiles.Update;

public static partial class UpdateProfile
{
    /// <summary>
    /// Validator for the <see cref="Request"/> to update profile fields.
    /// </summary>
    public sealed class Validator : AbstractValidator<Request>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        public Validator(ISystemDateTime systemDateTime)
        {
            // Validate: Apply individual field rules with optional constraints.
            RuleFor(x => x.FirstName).ApplyFirstNameRules(isRequired: false);
            RuleFor(x => x.LastName).ApplyLastNameRules(isRequired: false);
            RuleFor(x => x.DateOfBirth).ApplyDateOfBirthRules(systemDateTime);
        }
    }
}