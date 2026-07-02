using Shared.Governance.Conventions;

namespace Shared.Application.Domain.Concerns.Parameterizable;

public static class ParameterizableBehavior
{
    public static void ApplyNormalization(IParameterizable entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name) &&
            !string.IsNullOrWhiteSpace(entity.Presentation))
        {
            entity.Name = entity.Presentation;
        }

        if (!string.IsNullOrWhiteSpace(entity.Name))
        {
            entity.Name = Normalize(entity.Name);
        }
    }

    public static (string Name, string? Presentation) GetNormalizedValues(string name, string? presentation)
    {
        var normalized_name = Normalize(name);
        var normalized_presentation = string.IsNullOrWhiteSpace(presentation)
            ? null
            : Normalize(presentation);

        return (normalized_name, normalized_presentation);
    }

    public static string? ToNormalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant().ToSnakeCase();
    }

    public static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant().ToSnakeCase() ?? string.Empty;
    }
}