using System.Text.RegularExpressions;

namespace Module.Catalog.Domain.Products;

public static partial class ProductMethod
{
    #region Slug Generation
    public static string GenerateSlug(this Product product)
    {
        if (!string.IsNullOrEmpty(product.Slug))
        {
            return product.Slug;
        }

        return GenerateSlugFromName(product.Name);
    }

    public static string GenerateSlugFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Guid.NewGuid().ToString("N")[..8];
        }

        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-");

        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, @"-{2,}", "-");
        slug = slug.Trim('-');

        return string.IsNullOrEmpty(slug) ? Guid.NewGuid().ToString("N")[..8] : slug;
    }
    #endregion

    #region Slug Validation
    public static bool IsSlugAvailable(this Product product, string candidateSlug)
    {
        if (string.IsNullOrWhiteSpace(candidateSlug))
        {
            return false;
        }

        return !string.Equals(product.Slug, candidateSlug, StringComparison.OrdinalIgnoreCase);
    }

    public static string EnsureSlugIsUnique(this Product product, string candidateSlug)
    {
        if (string.IsNullOrWhiteSpace(candidateSlug))
        {
            return product.Slug;
        }

        if (product.IsSlugAvailable(candidateSlug))
        {
            return candidateSlug;
        }

        var unique = $"{candidateSlug}-{Guid.NewGuid():N}";
        return unique.Length > 255 ? unique[..255] : unique;
    }

    public static void NormalizeSlug(this Product product)
    {
        if (!string.IsNullOrEmpty(product.Slug))
        {
            product.Slug = product.Slug.ToLowerInvariant();
        }
    }
    #endregion
}