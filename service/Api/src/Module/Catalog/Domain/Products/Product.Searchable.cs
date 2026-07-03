namespace Module.Catalog.Domain.Products;

public static class ProductSearchableExtensions
{
    // Aggregate: Combine all searchable text fields into a single index string
    public static string SearchIndexText(this Product product)
    {
        var parts = new List<string>
        {
            product.Name,
            product.Description,
            product.Slug,
        };

        if (!string.IsNullOrEmpty(product.MetaKeywords))
        {
            parts.Add(product.MetaKeywords);
        }

        if (!string.IsNullOrEmpty(product.MetaDescription))
        {
            parts.Add(product.MetaDescription);
        }

        return string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }

    // Parse: Tokenize search index text into distinct lowercase search tokens
    public static string[] SearchTokens(this Product product)
    {
        return product.SearchIndexText()
            .ToLowerInvariant()
            .Split([' ', ',', '.', '-', '_', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .Distinct()
            .ToArray();
    }

    // Filter: Check if the product matches a given search query
    public static bool MatchesSearchQuery(this Product product, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        var lowerQuery = query.ToLowerInvariant();
        var indexText = product.SearchIndexText().ToLowerInvariant();

        return indexText.Contains(lowerQuery);
    }
}
