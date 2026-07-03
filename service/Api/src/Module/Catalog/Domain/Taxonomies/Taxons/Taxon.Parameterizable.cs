using Slugify;

namespace Module.Catalog.Domain.Taxonomies.Taxons;

public static class TaxonParameterizableExtensions
{
    private static readonly SlugHelper SlugHelper = new();

    // Compute: Return the display parameter for the taxon
    public static string ToParameter(this Taxon taxon)
    {
        if (string.IsNullOrWhiteSpace(taxon.Presentation))
        {
            return taxon.Name;
        }

        return taxon.Presentation;
    }

    // Generate: Convert taxon name to URL-safe slug
    public static string ToSlug(this Taxon taxon)
    {
        return SlugHelper.GenerateSlug(taxon.ToParameter());
    }

    // Compute: Build a full URL slug including parent hierarchy
    public static string ToUrlSlug(this Taxon taxon)
    {
        var slug = taxon.ToSlug();
        return taxon.Parent != null
            ? $"{taxon.Parent.Permalink}/{slug}"
            : slug;
    }
}
