namespace Shared.Security.Identity.Domain.Permissions;

/// <summary>
/// Success messages and error results for PermissionMetadata operations.
/// </summary>
public static class PermissionMetadataResult
{
    public static class Success
    {
        public const string Created = "Permission metadata created successfully.";
        public const string Parsed = "Permission identifier parsed successfully.";
        public const string Validated = "Permission metadata is valid.";
    }

    public static class Failure
    {
        public static Error PartTooLong(string partName, int length) => Error.Validation(
            code: $"Permission.{partName}.TooLong",
            message: $"{partName} exceeds maximum length of {PermissionMetadataConstant.Constraints.MaxPartLength}. (Provided: {length})");

        public static Error InvalidPartChars(string partName, string value) => Error.Validation(
            code: $"Permission.{partName}.InvalidChars",
            message: $"{partName} contains invalid characters. Allowed: {PermissionMetadataConstant.Constraints.AllowedPartChars}. (Value: '{value}')");

        public static Error PartRequired(string partName) => Error.Validation(
            code: $"Permission.{partName}.Required",
            message: $"{partName} is required and cannot be empty.");

        public static Error NameTooLong(int length) => Error.Validation(
            code: "Permission.Name.TooLong",
            message: $"Permission name exceeds maximum length of {PermissionMetadataConstant.Constraints.MaxNameLength}. (Provided: {length})");

        public static Error DescriptionTooLong(int length) => Error.Validation(
            code: "Permission.Description.TooLong",
            message: $"Permission description exceeds maximum length of {PermissionMetadataConstant.Constraints.MaxDescriptionLength}. (Provided: {length})");

        public static Error InvalidIdentifierFormat(string identifier) => Error.Validation(
            code: "Permission.Identifier.InvalidFormat",
            message: $"Invalid permission identifier format: '{identifier}'. Expected 4 dot‑separated parts with allowed characters.");

        public static Error IdentifierTooLong(int length) => Error.Validation(
            code: "Permission.Identifier.TooLong",
            message: $"Identifier exceeds maximum length of {PermissionMetadataConstant.Constraints.MaxIdentifierLength}. (Provided: {length})");

        public static Error ParseError(string identifier, string details) => Error.Validation(
            code: "Permission.Parse.Error",
            message: $"Failed to parse permission identifier '{identifier}': {details}");
    }
}