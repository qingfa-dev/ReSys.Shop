// Policy: Name length constraints follow RFC 5321 (email) and common full-name limits
// Invariant: All Max*Length constants are positive integers; Allowed*Values arrays are non-empty
namespace Module.Customer.Domain;

public static class UserProfileConstant
{
    public static class Defaults
    {
        public const bool IsActive = true;
    }

    public static class Constraints
    {
        public const int MaxFirstNameLength = 100;
        public const int MaxLastNameLength = 100;
        public const int MaxEmailLength = 255;
        public const int MaxPhoneNumberLength = 20;
        public const int MaxBioLength = 500;
        public const int MaxAvatarUrlLength = 500;
        public const int MaxGenderLength = 20;
        public const int MaxAddressesCount = 10;
        public const int MaxAddressesCountPerType = 5;
        public const int MaxInternalNoteLength = 5000;
        public const int TotalSpentPrecision = 18;
        public const int TotalSpentScale = 2;
    }

    public static class AllowedGenders
    {
        public static readonly string[] Values = ["Male", "Female", "Non-binary", "Prefer not to say"];
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(UserProfile.FirstName),
            nameof(UserProfile.LastName),
            nameof(UserProfile.Email),
            nameof(UserProfile.Bio)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(UserProfile.FirstName),
            nameof(UserProfile.LastName),
            nameof(UserProfile.CreatedAtUtc),
            nameof(UserProfile.ModifiedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(UserProfile.Gender),
            nameof(UserProfile.IsActive),
            nameof(UserProfile.CreatedAtUtc),
            nameof(UserProfile.ModifiedAtUtc)
        ];
    }
}