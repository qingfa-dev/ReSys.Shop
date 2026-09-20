using System.Text;

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
        return string.IsNullOrWhiteSpace(value) ? null : Normalize(value);
    }

    public static string Normalize(string value)
    {
        return ToSnakeCase(value.Trim()).ToLowerInvariant() ?? string.Empty;
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length);

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (c == '-')
            {
                sb.Append('_');
                continue;
            }

            if (char.IsUpper(c) && i > 0)
            {
                var prev = input[i - 1];
                if (!char.IsUpper(prev) || (i < input.Length - 1 && !char.IsUpper(input[i + 1])))
                {
                    sb.Append('_');
                }
            }

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}
