using System.Text.RegularExpressions;

namespace Shared.Security.Identity.Domain.Permissions;

/// <summary>
/// Contains constants, constraints, default templates, and helper methods for permissions.
/// </summary>
public static class PermissionMetadataConstant
{
    public const string ClaimType = "permission";
    public static class Constraints
    {
        public const int MaxPartLength = 50;
        public const int MaxIdentifierLength = 255;
        public const int MaxNameLength = 100;
        public const int MaxDescriptionLength = 500;
        public const string AllowedPartChars = "abcdefghijklmnopqrstuvwxyz0123456789_-";
    }

    public static class Patterns
    {
        private const string PartPattern = "[a-z0-9_-]{1,50}";

        public static readonly string PartRegex = $"^{PartPattern}$";
        public static readonly string IdentifierRegex = $"^{PartPattern}\\.{PartPattern}\\.{PartPattern}\\.{PartPattern}$";
    }

    public static class Defaults
    {
        public const string NameTemplate = "{Action} {Resource}";
        public const string DescriptionTemplate = "Allows {Action} on {Resource} in {Domain}/{Category}.";
    }

    /// <summary>Composition helpers.</summary>
    public static class Compose
    {
        public static string Identifier(string domain, string category, string resource, string action) =>
            string.Join('.', new[] { domain, category, resource, action }).ToLowerInvariant();

        public static string DefaultName(string action, string resource) => $"{action} {resource}";

        public static string DefaultDescription(string domain, string category, string resource, string action) =>
            Defaults.DescriptionTemplate
                .Replace("{Action}", action)
                .Replace("{Resource}", resource)
                .Replace("{Domain}", domain)
                .Replace("{Category}", category);
    }

    /// <summary>Checks identifier format without throwing.</summary>
    public static bool IsValidIdentifier(string identifier) =>
        !string.IsNullOrWhiteSpace(identifier) &&
        identifier.Length <= Constraints.MaxIdentifierLength &&
        Regex.IsMatch(identifier, Patterns.IdentifierRegex);
}