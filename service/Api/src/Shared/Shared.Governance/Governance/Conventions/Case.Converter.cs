using System.Text;

namespace Shared.Governance.Conventions;

public static class CaseConverter
{
    public static string ToPascalCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var separators = new[] { '_', '-' };
        var parts = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return input; // e.g. "___" – still a non‑null string

        var sb = new StringBuilder(input.Length);

        foreach (var part in parts)
        {
            if (part.Length == 0) continue;

            if (part.Length > 1 && part.All(char.IsUpper))
            {
                sb.Append(char.ToUpperInvariant(part[0]));
                sb.Append(part.AsSpan(1).ToString().ToLowerInvariant());
            }
            else
            {
                sb.Append(char.ToUpperInvariant(part[0]));
                if (part.Length > 1)
                {
                    sb.Append(part.AsSpan(1));
                }
            }
        }

        return sb.ToString();
    }

    public static string ToCamelCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var pascal = input.ToPascalCase();
        if (string.IsNullOrEmpty(pascal))
            return string.Empty;

        return char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
    }

    public static string ToSnakeCase(this string? input)
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

    public static string ToKebabCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length);

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (c == '_')
            {
                sb.Append('-');
                continue;
            }

            if (char.IsUpper(c) && i > 0)
            {
                var prev = input[i - 1];
                if (!char.IsUpper(prev) || (i < input.Length - 1 && !char.IsUpper(input[i + 1])))
                {
                    sb.Append('-');
                }
            }

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}