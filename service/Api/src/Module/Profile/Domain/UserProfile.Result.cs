// Contract: All error factories return Error instances with unique codes for traceability

namespace Module.Profile.Domain;

/// <summary>Contains success messages and error factory methods for UserProfile operations.</summary>
public static class UserProfileResult
{
    /// <summary>Success message factory for UserProfile operations.</summary>
    public static class Success
    {
        public const string ProfileRetrieved = "User profile retrieved successfully.";
        public const string ProfileCreated = "User profile created successfully.";
        public const string ProfileUpdated = "User profile updated successfully.";
    }

    /// <summary>Error factory methods returning typed Error instances for UserProfile operations.</summary>
    public static class Failure
    {
        /// <summary>User profile not found.</summary>
        public static Error UserNotFound => Error.NotFound(
            code: "UserProfile.UserNotFound",
            message: "User profile not found.");

        public static Error NotFound => Error.NotFound(
            code: "UserProfile.NotFound",
            message: "User profile not found.");

        public static Error AlreadyExists => Error.Conflict(
            code: "UserProfile.AlreadyExists",
            message: "User profile already exists for this user.");

        public static Error FirstNameRequired => Error.Validation(
            code: "UserProfile.FirstName.Required",
            message: "First name is required.");

        public static Error LastNameRequired => Error.Validation(
            code: "UserProfile.LastName.Required",
            message: "Last name is required.");

        public static Error EmailRequired => Error.Validation(
            code: "UserProfile.Email.Required",
            message: "Email is required.");

        public static Error FirstNameTooLong => Error.Validation(
            code: "UserProfile.FirstName.TooLong",
            message: $"First name cannot exceed {UserProfileConstant.Constraints.MaxFirstNameLength} characters.");

        public static Error LastNameTooLong => Error.Validation(
            code: "UserProfile.LastName.TooLong",
            message: $"Last name cannot exceed {UserProfileConstant.Constraints.MaxLastNameLength} characters.");

        public static Error EmailTooLong => Error.Validation(
            code: "UserProfile.Email.TooLong",
            message: $"Email cannot exceed {UserProfileConstant.Constraints.MaxEmailLength} characters.");

        public static Error PhoneNumberTooLong => Error.Validation(
            code: "UserProfile.PhoneNumber.TooLong",
            message:
            $"Phone number cannot exceed {UserProfileConstant.Constraints.MaxPhoneNumberLength} characters.");

        public static Error GenderTooLong => Error.Validation(
            code: "UserProfile.Gender.TooLong",
            message: $"Gender cannot exceed {UserProfileConstant.Constraints.MaxGenderLength} characters.");

        public static Error AvatarUrlTooLong => Error.Validation(
            code: "UserProfile.AvatarUrl.TooLong",
            message: $"Avatar URL cannot exceed {UserProfileConstant.Constraints.MaxAvatarUrlLength} characters.");

        public static Error InvalidGender => Error.Validation(
            code: "UserProfile.Gender.Invalid",
            message: "Invalid gender value.");

        public static Error BioTooLong => Error.Validation(
            code: "UserProfile.Bio.TooLong",
            message: $"Bio cannot exceed {UserProfileConstant.Constraints.MaxBioLength} characters.");

        public static Error InternalNoteTooLong => Error.Validation(
            code: "UserProfile.InternalNote.TooLong",
            message:
            $"Internal note cannot exceed {UserProfileConstant.Constraints.MaxInternalNoteLength} characters.");

        public static Error DateOfBirthFuture => Error.Validation(
            code: "UserProfile.DateOfBirth.Future",
            message: "Date of birth cannot be in the future.");

        public static Error DateOfBirthTooOld => Error.Validation(
            code: "UserProfile.DateOfBirth.TooOld",
            message: "Date of birth is too far in the past.");

        /// <summary>Authentication required for profile operations.</summary>
        public static Error AuthRequired => Error.Unauthorized(
            code: "UserProfile.AuthRequired",
            message: "Authentication required.");
    }
}