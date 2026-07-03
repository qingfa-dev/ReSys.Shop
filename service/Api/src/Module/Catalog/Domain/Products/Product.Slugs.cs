using System.Text.RegularExpressions;

namespace Module.Catalog.Domain.Products;

public static class ProductSlugsExtensions
{
    // Generate: Generate a URL slug from the product name
    public static string GenerateSlug(this Product product)
    {
        if (!string.IsNullOrEmpty(product.Slug))
        {
            return product.Slug;
        }

        return GenerateSlugFromName(product.Name);
    }

    // Normalize: Convert product name to URL-safe slug string
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

    // Check: Verify slug availability against current product slug
    public static bool IsSlugAvailable(this Product product, string candidateSlug)
    {
        if (string.IsNullOrWhiteSpace(candidateSlug))
        {
            return false;
        }

        return !string.Equals(product.Slug, candidateSlug, StringComparison.OrdinalIgnoreCase);
    }

    // Guard: Ensure slug uniqueness by appending a UUID if colliding
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

        return $"{candidateSlug}-{Guid.NewGuid():N}"[..255];
    }

    // Normalize: Downcase the product slug
    public static void NormalizeSlug(this Product product)
    {
        if (!string.IsNullOrEmpty(product.Slug))
        {
            product.Slug = product.Slug.ToLowerInvariant();
        }
    }
}
