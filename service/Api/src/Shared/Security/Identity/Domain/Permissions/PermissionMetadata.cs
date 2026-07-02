using FluentValidation;
using FluentValidation.Results;

using Shared.Application.Mappings;

namespace Shared.Security.Identity.Domain.Permissions;

public sealed record PermissionMetadata : IDescriptor
{
    private static readonly PermissionMetadataValidator s_validator = new();

    public string Domain { get; }
    public string Category { get; }
    public string Resource { get; }
    public string Action { get; }
    public string Identifier { get; }
    public string Name { get; init; }
    public string Description { get; init; }
    public object? Example { get; init; }

    internal PermissionMetadata(
        string domain,
        string category,
        string resource,
        string action,
        string? name = null,
        string? description = null,
        object? example = null,
        string? identifier = null)
    {
        Domain = domain.Trim();
        Category = category.Trim();
        Resource = resource.Trim();
        Action = action.Trim();

        Name = string.IsNullOrWhiteSpace(name)
            ? PermissionMetadataConstant.Compose.DefaultName(Action, Resource)
            : name.Trim();

        Description = string.IsNullOrWhiteSpace(description)
            ? PermissionMetadataConstant.Compose.DefaultDescription(Domain, Category, Resource, Action)
            : description.Trim();

        Example = example;

        Identifier = identifier ?? PermissionMetadataConstant.Compose.Identifier(Domain, Category, Resource, Action);
    }

    // ----- Throwing factories -----
    public static PermissionMetadata Create(string domain, string category, string resource, string action,
        string? name = null, string? description = null, object? example = null)
    {
        var metadata = new PermissionMetadata(domain, category, resource, action, name, description, example);
        s_validator.ValidateAndThrow(metadata);
        return metadata;
    }

    public static PermissionMetadata Parse(string identifier)
    {
        if (TryParse(identifier, out PermissionMetadata? permission))
            return permission!;
        throw new FormatException(PermissionMetadataResult.Failure.InvalidIdentifierFormat(identifier).Message);
    }

    public static bool TryParse(string identifier, out PermissionMetadata? permission)
    {
        permission = null;
        if (!PermissionMetadataConstant.IsValidIdentifier(identifier))
            return false;

        var parts = identifier.Split('.');
        permission = Create(parts[0], parts[1], parts[2], parts[3]);
        return true;
    }

    // ----- Result‑based factories -----
    public static Result<PermissionMetadata> TryCreate(
        string domain, string category, string resource, string action,
        string? name = null, string? description = null, object? example = null)
    {
        var metadata = new PermissionMetadata(domain, category, resource, action, name, description, example);
        ValidationResult validationResult = s_validator.Validate(metadata);
        if (!validationResult.IsValid)
            return validationResult.ToErrors<PermissionMetadata>();

        return metadata;
    }

    public static Result<PermissionMetadata> TryParse(string identifier)
    {
        if (!PermissionMetadataConstant.IsValidIdentifier(identifier))
            return PermissionMetadataResult.Failure.InvalidIdentifierFormat(identifier);

        var parts = identifier.Split('.');
        return TryCreate(parts[0], parts[1], parts[2], parts[3]);
    }

    public override string ToString() => Identifier;
    public static implicit operator string(PermissionMetadata permission) => permission.Identifier;
}
