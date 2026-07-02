// Policy: Name length constraints follow RFC 5321 (email) and common full-name limits
// Invariant: All Max*Length constants are positive integers; Allowed*Values arrays are non-empty
namespace Module.Profile.Domain;

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
    }

    public static class AllowedGenders
    {
        public static readonly string[] Values = ["Male", "Female", "Non-binary", "Prefer not to say"];
    }
}