namespace Shared.Security.Identity.Domain.Permissions;

/// <summary>
/// Additional static helpers for PermissionMetadata.
/// </summary>
public static class PermissionMetadataMethod
{
    // ----- Throwing versions (delegate to PermissionMetadata) -----
    public static PermissionMetadata For(string domain, string category, string resource, string action,
        string? name = null, string? description = null, object? example = null) =>
        PermissionMetadata.Create(domain, category, resource, action, name, description, example);

    // ----- Result‑based versions -----
    public static Result<PermissionMetadata> TryFor(string domain, string category, string resource, string action,
        string? name = null, string? description = null, object? example = null) =>
        PermissionMetadata.TryCreate(domain, category, resource, action, name, description, example);

    // ----- Customisation helpers (throwing) -----
    public static PermissionMetadata WithName(this PermissionMetadata metadata, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace.");
        if (name.Length > PermissionMetadataConstant.Constraints.MaxNameLength)
            throw new ArgumentException($"Name exceeds max length of {PermissionMetadataConstant.Constraints.MaxNameLength}.");
        return metadata with { Name = name.Trim() };
    }

    public static PermissionMetadata WithDescription(this PermissionMetadata metadata, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or whitespace.");
        if (description.Length > PermissionMetadataConstant.Constraints.MaxDescriptionLength)
            throw new ArgumentException($"Description exceeds max length of {PermissionMetadataConstant.Constraints.MaxDescriptionLength}.");
        return metadata with { Description = description.Trim() };
    }

    public static PermissionMetadata WithExample(this PermissionMetadata metadata, object? example) =>
        metadata with { Example = example };

    // ----- Result‑based customisation -----
    public static Result<PermissionMetadata> TryWithName(this PermissionMetadata metadata, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return PermissionMetadataResult.Failure.PartRequired("Name");
        if (name.Length > PermissionMetadataConstant.Constraints.MaxNameLength)
            return PermissionMetadataResult.Failure.NameTooLong(name.Length);
        return metadata with { Name = name.Trim() };
    }

    public static Result<PermissionMetadata> TryWithDescription(this PermissionMetadata metadata, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return PermissionMetadataResult.Failure.PartRequired("Description");
        if (description.Length > PermissionMetadataConstant.Constraints.MaxDescriptionLength)
            return PermissionMetadataResult.Failure.DescriptionTooLong(description.Length);
        return metadata with { Description = description.Trim() };
    }

    public static Result<PermissionMetadata> TryWithExample(this PermissionMetadata metadata, object? example) =>
        metadata with { Example = example };

    // ----- Parse helpers (throwing & result) -----
    public static PermissionMetadata Parse(string identifier) => PermissionMetadata.Parse(identifier);
    public static bool TryParse(string identifier, out PermissionMetadata? permission) =>
        PermissionMetadata.TryParse(identifier, out permission);
    public static Result<PermissionMetadata> TryParseResult(string identifier) =>
        PermissionMetadata.TryParse(identifier);

    // ----- Validation -----
    public static bool IsValidIdentifier(string identifier) =>
        PermissionMetadataConstant.IsValidIdentifier(identifier);
}
